using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Services;

internal class AppointmentReader : IAppointmentReader
{
    private readonly AgendaDbContext _context;

    public AppointmentReader(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AppointmentHistoryDto>> GetOwnerHistoryAsync(
        Guid ownerId, int limit, CancellationToken ct)
    {
        // ownerId maps to AnimalId — the animal (patient) belonging to the owner.
        // The multi-tenant global query filter applies automatically.
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.AnimalId == ownerId)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.StartTime)
            .Take(limit)
            .Select(a => new AppointmentHistoryDto(
                a.Status,
                a.Date,
                a.DurationMinutes,
                a.Reason,
                a.Status == AppointmentStatus.NoShow,
                false)) // ReminderSent not tracked in current domain model
            .ToListAsync(ct);

        return appointments;
    }

    public async Task<AppointmentFeaturesDto?> GetFeaturesForPredictionAsync(
        Guid appointmentId, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Id == appointmentId)
            .FirstOrDefaultAsync(ct);

        if (appointment is null)
            return null;

        var ownerHistory = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.AnimalId == appointment.AnimalId && a.Id != appointmentId)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.StartTime)
            .Select(a => new { a.Date, a.Status })
            .ToListAsync(ct);

        var noShowCount = ownerHistory.Count(h => h.Status == AppointmentStatus.NoShow);
        var totalAppointments = ownerHistory.Count;

        var lastVisit = ownerHistory
            .Where(h => h.Date < appointment.Date)
            .OrderByDescending(h => h.Date)
            .FirstOrDefault();

        var daysSinceLastVisit = lastVisit is not null
            ? (appointment.Date.ToDateTime(TimeOnly.MinValue) - lastVisit.Date.ToDateTime(TimeOnly.MinValue)).Days
            : 0;

        return new AppointmentFeaturesDto(
            AppointmentId: appointment.Id,
            AnimalId: appointment.AnimalId,
            Date: appointment.Date,
            StartTime: appointment.StartTime,
            ConsultationType: appointment.Reason,
            WasReminderSent: false, // ReminderSent not tracked in current domain model
            OwnerTotalAppointments: totalAppointments,
            OwnerNoShowCount: noShowCount,
            DaysSinceLastVisit: daysSinceLastVisit,
            LeadTimeDays: 0); // CreatedAt not tracked in current domain model
    }

    public async Task<IReadOnlyList<AppointmentFeaturesDto>> GetAppointmentsByDateAsync(
        DateOnly date, CancellationToken ct)
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date == date)
            .OrderBy(a => a.StartTime)
            .Take(200)
            .Select(a => new { a.Id, a.AnimalId, a.Date, a.StartTime, a.Reason })
            .ToListAsync(ct);

        if (appointments.Count == 0)
            return [];

        var animalIds = appointments.Select(a => a.AnimalId).Distinct().ToList();

        // Load history for all relevant animals in one query
        var historyByAnimal = await _context.Appointments
            .AsNoTracking()
            .Where(a => animalIds.Contains(a.AnimalId) && a.Date < date)
            .GroupBy(a => a.AnimalId)
            .Select(g => new
            {
                AnimalId = g.Key,
                Total = g.Count(),
                NoShows = g.Count(x => x.Status == AppointmentStatus.NoShow),
                LastVisitDate = g.Max(x => (DateOnly?)x.Date)
            })
            .ToListAsync(ct);

        var historyMap = historyByAnimal.ToDictionary(h => h.AnimalId);

        var result = appointments.Select(a =>
        {
            historyMap.TryGetValue(a.AnimalId, out var hist);
            var total = hist?.Total ?? 0;
            var noShows = hist?.NoShows ?? 0;
            var lastVisit = hist?.LastVisitDate;
            var daysSince = lastVisit.HasValue
                ? (a.Date.ToDateTime(TimeOnly.MinValue) - lastVisit.Value.ToDateTime(TimeOnly.MinValue)).Days
                : 0;

            return new AppointmentFeaturesDto(
                AppointmentId: a.Id,
                AnimalId: a.AnimalId,
                Date: a.Date,
                StartTime: a.StartTime,
                ConsultationType: a.Reason,
                WasReminderSent: false,
                OwnerTotalAppointments: total,
                OwnerNoShowCount: noShows,
                DaysSinceLastVisit: daysSince,
                LeadTimeDays: 0);
        }).ToList();

        return result;
    }

    public async Task<int> GetCompletedAppointmentCountAsync(CancellationToken ct)
    {
        return await _context.Appointments
            .AsNoTracking()
            .CountAsync(a => a.Status == AppointmentStatus.Completed
                          || a.Status == AppointmentStatus.NoShow, ct);
    }
}
