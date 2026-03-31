using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Infrastructure;

internal class AppointmentStatsReader : IAppointmentStatsReader
{
    private readonly AgendaDbContext _context;

    public AppointmentStatsReader(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyDictionary<Guid, int>>> GetAppointmentCountsByClinicIdsAsync(
        IReadOnlyList<Guid> clinicIds, CancellationToken ct = default)
    {
        if (clinicIds.Count == 0)
            return Result<IReadOnlyDictionary<Guid, int>>.Success(
                new Dictionary<Guid, int>());

        var counts = await _context.Appointments
            .IgnoreQueryFilters()
            .Where(a => clinicIds.Contains(a.ClinicId))
            .GroupBy(a => a.ClinicId)
            .Select(g => new { ClinicId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var result = clinicIds.ToDictionary(
            id => id,
            id => counts.FirstOrDefault(c => c.ClinicId == id)?.Count ?? 0);

        return Result<IReadOnlyDictionary<Guid, int>>.Success(result);
    }
}
