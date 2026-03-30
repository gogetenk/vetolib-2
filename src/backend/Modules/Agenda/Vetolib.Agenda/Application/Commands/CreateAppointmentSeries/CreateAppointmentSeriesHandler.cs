using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CreateAppointmentSeries;

internal class CreateAppointmentSeriesHandler : IRequestHandler<CreateAppointmentSeriesCommand, Result<List<AppointmentDto>>>
{
    private readonly AgendaDbContext _context;

    public CreateAppointmentSeriesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(CreateAppointmentSeriesCommand cmd, CancellationToken ct)
    {
        // Validate recurrence rule
        var ruleResult = RecurrenceRule.Create(cmd.Frequency, cmd.Count);
        if (!ruleResult.IsSuccess)
            return Result<List<AppointmentDto>>.Invalid(ruleResult.ValidationErrors.ToList());

        var rule = ruleResult.Value;
        var dates = rule.GenerateDates(cmd.StartDate);
        var seriesId = Guid.NewGuid();
        var schedule = ClinicSchedule.Default;
        var endTime = cmd.StartTime.AddMinutes(cmd.DurationMinutes);

        // Validate business hours (same for all appointments in the series)
        if (!schedule.IsWithinBusinessHours(cmd.StartTime, endTime))
        {
            return Result<List<AppointmentDto>>.Error(
                $"OUTSIDE_BUSINESS_HOURS:This time slot is outside of business hours ({schedule.FormatHours()})");
        }

        // Validate: start date must not be in the past
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (cmd.StartDate < today)
        {
            return Result<List<AppointmentDto>>.Error("PAST_DATE_NOT_ALLOWED:Cannot create an appointment series starting in the past");
        }

        // Check conflicts for all dates
        var firstDate = dates.First();
        var lastDate = dates.Last();
        var existingAppointments = await _context.Appointments
            .Where(a => a.VeterinarianId == cmd.VeterinarianId
                        && a.Date >= firstDate
                        && a.Date <= lastDate
                        && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync(ct);

        var conflictDates = new List<DateOnly>();
        foreach (var date in dates)
        {
            var dayAppointments = existingAppointments.Where(a => a.Date == date);
            if (dayAppointments.Any(a => a.OverlapsWith(cmd.StartTime, endTime)))
            {
                conflictDates.Add(date);
            }
        }

        if (conflictDates.Count > 0)
        {
            var conflictList = string.Join(", ", conflictDates.Select(d => d.ToString("yyyy-MM-dd")));
            return Result<List<AppointmentDto>>.Error(
                $"APPOINTMENT_CONFLICT:Time slot conflicts on the following dates: {conflictList}");
        }

        // Create all appointments
        var appointments = new List<Appointment>();
        foreach (var date in dates)
        {
            var appointmentResult = Appointment.Create(
                cmd.ClinicId,
                cmd.VeterinarianId,
                cmd.VeterinarianName,
                cmd.AnimalId,
                cmd.AnimalName,
                cmd.OwnerName,
                cmd.OwnerEmail,
                date,
                cmd.StartTime,
                cmd.DurationMinutes,
                cmd.Reason);

            if (!appointmentResult.IsSuccess)
                return Result<List<AppointmentDto>>.Invalid(appointmentResult.ValidationErrors.ToList());

            appointmentResult.Value.AssignToSeries(seriesId);
            appointments.Add(appointmentResult.Value);
        }

        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync(ct);

        return Result<List<AppointmentDto>>.Success(appointments.Select(a => a.ToDto()).ToList());
    }
}
