using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetWeightCurve;

internal class GetWeightCurveHandler : IRequestHandler<GetWeightCurveQuery, Result<IReadOnlyList<WeightCurvePointDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetWeightCurveHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<WeightCurvePointDto>>> Handle(GetWeightCurveQuery query, CancellationToken ct)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == query.PatientId, ct);

        if (!patientExists)
            return Result<IReadOnlyList<WeightCurvePointDto>>.NotFound($"Patient '{query.PatientId}' not found.");

        var points = await _context.WeightEntries
            .AsNoTracking()
            .Where(w => w.PatientId == query.PatientId)
            .OrderBy(w => w.RecordedAt)
            .Select(w => new WeightCurvePointDto(w.RecordedAt, w.WeightKg))
            .ToListAsync(ct);

        return Result<IReadOnlyList<WeightCurvePointDto>>.Success(points);
    }
}
