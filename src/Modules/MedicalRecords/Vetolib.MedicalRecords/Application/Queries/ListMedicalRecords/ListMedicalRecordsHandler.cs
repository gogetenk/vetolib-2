using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.ListMedicalRecords;

internal class ListMedicalRecordsHandler : IRequestHandler<ListMedicalRecordsQuery, Result<List<MedicalRecordDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public ListMedicalRecordsHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MedicalRecordDto>>> Handle(ListMedicalRecordsQuery query, CancellationToken ct)
    {
        var records = await _context.MedicalRecords
            .Where(r => r.PatientId == query.PatientId)
            .Include(r => r.Prescriptions)
            .OrderByDescending(r => r.ExaminedAt)
            .ToListAsync(ct);

        return Result<List<MedicalRecordDto>>.Success(records.Select(r => r.ToDto()).ToList());
    }
}
