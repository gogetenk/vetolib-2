using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CreateBookingAppointment;

internal class CreateBookingAppointmentHandler
    : IRequestHandler<CreateBookingAppointmentCommand, Result<AppointmentDto>>
{
    /// <summary>
    /// Maximum number of days in advance an owner can book via the portal.
    /// </summary>
    private const int MaxAdvanceDays = 90;

    private readonly AgendaDbContext _context;

    public CreateBookingAppointmentHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateBookingAppointmentCommand cmd, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Validate: date must not be in the past
        if (cmd.Date < today)
            return Result<AppointmentDto>.Error(
                "PAST_DATE_NOT_ALLOWED:Cannot book an appointment in the past");

        // Validate: date must not be beyond MaxAdvanceDays
        var maxDate = today.AddDays(MaxAdvanceDays);
        if (cmd.Date > maxDate)
            return Result<AppointmentDto>.Error(
                $"DATE_TOO_FAR:Cannot book more than {MaxAdvanceDays} days in advance");

        // Validate: must be within business hours
        var schedule = ClinicSchedule.Default;
        var endTime = cmd.StartTime.AddMinutes(cmd.DurationMinutes);
        if (!schedule.IsWithinBusinessHours(cmd.StartTime, endTime))
            return Result<AppointmentDto>.Error(
                $"OUTSIDE_BUSINESS_HOURS:Slot is outside operating hours ({schedule.FormatHours()})");

        // Validate: no conflict with existing appointments for the same vet on the same day
        // IgnoreQueryFilters() because portal request has no tenant context in IClinicContext.
        // We explicitly filter by ClinicId for isolation.
        var conflictingAppointments = await _context.Appointments
            .IgnoreQueryFilters()
            .Where(a => a.ClinicId == cmd.ClinicId
                        && a.VeterinarianId == cmd.VeterinarianId
                        && a.Date == cmd.Date
                        && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync(ct);

        var hasConflict = conflictingAppointments.Any(a => a.OverlapsWith(cmd.StartTime, endTime));
        if (hasConflict)
            return Result<AppointmentDto>.Error(
                "APPOINTMENT_CONFLICT:This slot is already taken for the selected veterinarian");

        // Create appointment via domain factory with OwnerPortal source
        var appointmentResult = Appointment.Create(
            cmd.ClinicId,
            cmd.VeterinarianId,
            cmd.VeterinarianName,
            cmd.AnimalId,
            cmd.AnimalName,
            cmd.OwnerName,
            cmd.Date,
            cmd.StartTime,
            cmd.DurationMinutes,
            cmd.Reason,
            source: BookingSource.OwnerPortal,
            ownerId: cmd.OwnerId);

        if (!appointmentResult.IsSuccess)
            return Result<AppointmentDto>.Invalid(appointmentResult.ValidationErrors.ToList());

        _context.Appointments.Add(appointmentResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointmentResult.Value.ToDto());
    }
}
