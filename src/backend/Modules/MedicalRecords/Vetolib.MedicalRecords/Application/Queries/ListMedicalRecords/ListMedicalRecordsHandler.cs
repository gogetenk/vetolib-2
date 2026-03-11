using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.ListMedicalRecords;

internal class ListMedicalRecordsHandler : IRequestHandler<ListMedicalRecordsQuery, Result<MedicalRecordPagedResultDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public ListMedicalRecordsHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MedicalRecordPagedResultDto>> Handle(ListMedicalRecordsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var baseQuery = _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId)
            .OrderByDescending(r => r.ExaminedAt);

        var totalCount = await baseQuery.CountAsync(ct);

        var records = await baseQuery
            .Include(r => r.Prescriptions)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new MedicalRecordPagedResultDto(
            records.Select(r => r.ToDto()).ToList(),
            totalCount,
            page,
            pageSize);

        return Result<MedicalRecordPagedResultDto>.Success(result);
    }
}
