using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientCount;

internal class GetPatientCountHandler : IRequestHandler<GetPatientCountQuery, Result<int>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientCountHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(GetPatientCountQuery query, CancellationToken ct)
    {
        var count = await _context.Patients
            .AsNoTracking()
            .CountAsync(ct);

        return Result<int>.Success(count);
    }
}
