using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalRecords;

internal class GetAnimalRecordsHandler : IRequestHandler<GetAnimalRecordsQuery, Result<IReadOnlyList<PortalMedicalRecordDto>>>
{
    private readonly IOwnerAuthorizationService _ownerAuth;
    private readonly MedicalRecordsDbContext _context;

    public GetAnimalRecordsHandler(IOwnerAuthorizationService ownerAuth, MedicalRecordsDbContext context)
    {
        _ownerAuth = ownerAuth;
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PortalMedicalRecordDto>>> Handle(GetAnimalRecordsQuery query, CancellationToken ct)
    {
        var authResult = await _ownerAuth.IsOwnerLinkedToPatient(query.OwnerAccountId, query.PatientId, ct);
        if (!authResult.IsSuccess)
            return Result<IReadOnlyList<PortalMedicalRecordDto>>.Invalid(authResult.ValidationErrors.ToList());

        if (!authResult.Value)
            return Result<IReadOnlyList<PortalMedicalRecordDto>>.Forbidden();

        var records = await _context.MedicalRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId && r.IsVisibleToOwner)
            .OrderByDescending(r => r.ExaminedAt)
            .Select(r => new PortalMedicalRecordDto(
                r.Id,
                r.Diagnosis,
                r.Treatment,
                r.VetName,
                r.ExaminedAt))
            .ToListAsync(ct);

        return Result<IReadOnlyList<PortalMedicalRecordDto>>.Success(records);
    }
}
