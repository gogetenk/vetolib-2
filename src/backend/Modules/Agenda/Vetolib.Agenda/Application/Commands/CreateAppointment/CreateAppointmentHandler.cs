using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CreateAppointment;

internal class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly AgendaDbContext _context;

    public CreateAppointmentHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand cmd, CancellationToken ct)
    {
        // Validate: date must not be in the past
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (cmd.Date < today)
        {
            return Result<AppointmentDto>.Error("PAST_DATE_NOT_ALLOWED:Cannot create an appointment in the past");
        }

        // Validate: must be within business hours
        var schedule = ClinicSchedule.Default;
        var endTime = cmd.StartTime.AddMinutes(cmd.DurationMinutes);
        if (!schedule.IsWithinBusinessHours(cmd.StartTime, endTime))
        {
            return Result<AppointmentDto>.Error(
                $"OUTSIDE_BUSINESS_HOURS:This time slot is outside of business hours ({schedule.FormatHours()})");
        }

        // Validate: no conflict with existing appointments for the same vet on the same day
        var conflictingAppointments = await _context.Appointments
            .Where(a => a.VeterinarianId == cmd.VeterinarianId
                        && a.Date == cmd.Date
                        && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync(ct);

        var conflict = conflictingAppointments
            .FirstOrDefault(a => a.OverlapsWith(cmd.StartTime, endTime));

        if (conflict is not null)
        {
            // Find next available slots
            var nextSlots = FindNextAvailableSlots(conflictingAppointments, cmd.StartTime, cmd.DurationMinutes, schedule, 3);
            var slotsText = nextSlots.Count > 0
                ? " Next available slots: " + string.Join(", ", nextSlots.Select(s => s.ToString("HH:mm")))
                : "";

            return Result<AppointmentDto>.Error(
                $"APPOINTMENT_CONFLICT:This time slot is already taken for this veterinarian.{slotsText}");
        }

        // Create appointment via domain factory
        var appointmentResult = Appointment.Create(
            cmd.ClinicId,
            cmd.VeterinarianId,
            cmd.VeterinarianName,
            cmd.AnimalId,
            cmd.AnimalName,
            cmd.OwnerName,
            cmd.OwnerEmail,
            cmd.Date,
            cmd.StartTime,
            cmd.DurationMinutes,
            cmd.Reason);

        if (!appointmentResult.IsSuccess)
            return Result<AppointmentDto>.Invalid(appointmentResult.ValidationErrors.ToList());

        _context.Appointments.Add(appointmentResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointmentResult.Value.ToDto());
    }

    private static List<TimeOnly> FindNextAvailableSlots(
        List<Appointment> existingAppointments,
        TimeOnly requestedTime,
        int durationMinutes,
        ClinicSchedule schedule,
        int maxSlots)
    {
        var slots = new List<TimeOnly>();
        var candidateTime = requestedTime;

        // Try slots in 15-minute increments after the requested time
        for (int i = 0; i < 100 && slots.Count < maxSlots; i++)
        {
            candidateTime = candidateTime.AddMinutes(15);
            var candidateEnd = candidateTime.AddMinutes(durationMinutes);

            if (!schedule.IsWithinBusinessHours(candidateTime, candidateEnd))
                break;

            var hasConflict = existingAppointments
                .Any(a => a.OverlapsWith(candidateTime, candidateEnd));

            if (!hasConflict)
                slots.Add(candidateTime);
        }

        return slots;
    }
}
