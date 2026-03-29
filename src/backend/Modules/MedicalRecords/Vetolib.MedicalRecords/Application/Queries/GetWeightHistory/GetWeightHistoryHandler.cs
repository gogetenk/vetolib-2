using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetWeightHistory;

internal class GetWeightHistoryHandler : IRequestHandler<GetWeightHistoryQuery, Result<WeightHistoryResultDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetWeightHistoryHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WeightHistoryResultDto>> Handle(GetWeightHistoryQuery query, CancellationToken ct)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == query.PatientId, ct);

        if (!patientExists)
            return Result<WeightHistoryResultDto>.NotFound($"Patient '{query.PatientId}' not found.");

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var baseQuery = _context.WeightEntries
            .AsNoTracking()
            .Where(w => w.PatientId == query.PatientId)
            .OrderByDescending(w => w.RecordedAt);

        var totalCount = await baseQuery.CountAsync(ct);

        var entries = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new WeightHistoryResultDto(
            entries.Select(e => e.ToDto()).ToList(),
            totalCount,
            page,
            pageSize);

        return Result<WeightHistoryResultDto>.Success(result);
    }
}
