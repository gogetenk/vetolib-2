using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientsBySpecies;

internal class GetPatientsBySpeciesHandler
    : IRequestHandler<GetPatientsBySpeciesQuery, Result<IReadOnlyList<PatientsBySpeciesDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientsBySpeciesHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PatientsBySpeciesDto>>> Handle(
        GetPatientsBySpeciesQuery query, CancellationToken ct)
    {
        var rows = await _context.Patients
            .AsNoTracking()
            .GroupBy(p => p.Species)
            .Select(g => new { Species = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync(ct);

        var result = rows
            .Select(r => new PatientsBySpeciesDto(
                Species: r.Species.ToString(),
                Count: r.Count))
            .ToList();

        return Result<IReadOnlyList<PatientsBySpeciesDto>>.Success(result);
    }
}
