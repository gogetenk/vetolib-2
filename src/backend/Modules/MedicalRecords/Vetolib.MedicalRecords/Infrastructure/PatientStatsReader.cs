using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientStatsReader : IPatientStatsReader
{
    private readonly MedicalRecordsDbContext _context;

    public PatientStatsReader(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyDictionary<Guid, int>>> GetPatientCountsByClinicIdsAsync(
        IReadOnlyList<Guid> clinicIds, CancellationToken ct = default)
    {
        if (clinicIds.Count == 0)
            return Result<IReadOnlyDictionary<Guid, int>>.Success(
                new Dictionary<Guid, int>());

        var counts = await _context.Patients
            .IgnoreQueryFilters()
            .Where(p => clinicIds.Contains(p.ClinicId))
            .GroupBy(p => p.ClinicId)
            .Select(g => new { ClinicId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var result = clinicIds.ToDictionary(
            id => id,
            id => counts.FirstOrDefault(c => c.ClinicId == id)?.Count ?? 0);

        return Result<IReadOnlyDictionary<Guid, int>>.Success(result);
    }
}
