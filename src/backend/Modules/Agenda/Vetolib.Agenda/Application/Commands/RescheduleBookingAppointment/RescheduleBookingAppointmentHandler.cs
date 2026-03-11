using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.RescheduleBookingAppointment;

internal class RescheduleBookingAppointmentHandler : IRequestHandler<RescheduleBookingAppointmentCommand, Result<AppointmentDto>>
{
    // Default maximum reschedules allowed per booking (without clinic-specific preferences)
    private const int DefaultMaxReschedules = 2;

    // Default minimum hours before appointment that owner can reschedule
    private const int DefaultMinRescheduleHours = 24;

    private readonly AgendaDbContext _context;

    public RescheduleBookingAppointmentHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(RescheduleBookingAppointmentCommand cmd, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result<AppointmentDto>.NotFound($"Appointment '{cmd.AppointmentId}' not found.");

        // Verify ownership
        if (appointment.OwnerId != cmd.OwnerId)
            return Result<AppointmentDto>.Forbidden();

        // Verify status allows rescheduling
        if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.CheckedIn)
            return Result<AppointmentDto>.Error(
                $"INVALID_STATUS:Cannot reschedule an appointment in status '{appointment.Status}'. Only Scheduled or CheckedIn appointments can be rescheduled.");

        // Verify reschedule delay window (BookingMinCancelHours, default 24h)
        var appointmentDateTime = appointment.Date.ToDateTime(appointment.StartTime);
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;

        if (hoursUntilAppointment < DefaultMinRescheduleHours)
            return Result<AppointmentDto>.Error(
                $"RESCHEDULE_WINDOW_EXPIRED:Appointments can only be rescheduled at least {DefaultMinRescheduleHours} hours in advance. This appointment starts in {Math.Floor(hoursUntilAppointment)} hours.");

        // Verify reschedule limit (BookingMaxReschedules, default 2)
        var incrementResult = appointment.IncrementReschedule(Guid.Empty, DefaultMaxReschedules);
        if (!incrementResult.IsSuccess)
            return Result<AppointmentDto>.Error(incrementResult.Errors.First());

        // Verify new slot: new date must be in the future
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (cmd.NewDate < today)
            return Result<AppointmentDto>.Error("PAST_DATE_NOT_ALLOWED:New date cannot be in the past.");

        // Check for conflicts on new slot
        var newEndTime = cmd.NewStartTime.AddMinutes(cmd.NewDurationMinutes);
        var conflictingAppointments = await _context.Appointments
            .Where(a => a.VeterinarianId == appointment.VeterinarianId
                        && a.Date == cmd.NewDate
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Id != appointment.Id)
            .ToListAsync(ct);

        var conflict = conflictingAppointments.FirstOrDefault(a => a.OverlapsWith(cmd.NewStartTime, newEndTime));
        if (conflict is not null)
            return Result<AppointmentDto>.Error("APPOINTMENT_CONFLICT:The requested new time slot is not available for this veterinarian.");

        // Cancel the original appointment
        var cancelResult = appointment.Cancel("Rescheduled by owner via portal");
        if (!cancelResult.IsSuccess)
            return Result<AppointmentDto>.Error(cancelResult.Errors.First());

        // Create the new appointment with RescheduleCount+1 and OriginalAppointmentId
        var newAppointmentResult = Appointment.Create(
            appointment.ClinicId,
            appointment.VeterinarianId,
            appointment.VeterinarianName,
            appointment.AnimalId,
            appointment.AnimalName,
            appointment.OwnerName,
            cmd.NewDate,
            cmd.NewStartTime,
            cmd.NewDurationMinutes,
            appointment.Reason,
            BookingSource.OwnerPortal,
            appointment.OwnerId);

        if (!newAppointmentResult.IsSuccess)
            return Result<AppointmentDto>.Invalid(newAppointmentResult.ValidationErrors.ToList());

        var newAppointment = newAppointmentResult.Value;
        newAppointment.SetOriginalAppointmentId(appointment.Id);

        // RescheduleCount on new appointment = original.RescheduleCount (already incremented above)
        for (int i = 0; i < appointment.RescheduleCount; i++)
            newAppointment.IncrementReschedule(Guid.Empty, DefaultMaxReschedules);

        _context.Appointments.Add(newAppointment);
        await _context.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(newAppointment.ToDto());
    }
}
