using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetAvailability;

internal class GetAvailabilityHandler
    : IRequestHandler<GetAvailabilityQuery, Result<IReadOnlyList<AvailabilitySlotDto>>>
{
    private readonly AgendaDbContext _context;

    public GetAvailabilityHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<AvailabilitySlotDto>>> Handle(
        GetAvailabilityQuery query, CancellationToken ct)
    {
        var existingAppointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.VeterinarianId == query.VeterinarianId
                        && a.Date == query.Date
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Status != AppointmentStatus.NoShow)
            .ToListAsync(ct);

        var schedule = ClinicSchedule.Default;
        var slots = BuildSlots(existingAppointments, schedule, query.DurationMinutes);

        return Result<IReadOnlyList<AvailabilitySlotDto>>.Success(slots);
    }

    private static IReadOnlyList<AvailabilitySlotDto> BuildSlots(
        List<Appointment> existingAppointments,
        ClinicSchedule schedule,
        int durationMinutes)
    {
        var slots = new List<AvailabilitySlotDto>();
        var current = schedule.OpeningTime;

        while (true)
        {
            var end = current.AddMinutes(durationMinutes);
            if (!schedule.IsWithinBusinessHours(current, end))
                break;

            var isAvailable = !existingAppointments.Any(a => a.OverlapsWith(current, end));
            slots.Add(new AvailabilitySlotDto(current, end, isAvailable));

            current = current.AddMinutes(30); // 30-minute grid
        }

        return slots;
    }
}
