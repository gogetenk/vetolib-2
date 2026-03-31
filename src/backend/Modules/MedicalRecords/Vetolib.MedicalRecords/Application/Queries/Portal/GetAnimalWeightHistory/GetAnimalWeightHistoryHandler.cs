using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalWeightHistory;

internal class GetAnimalWeightHistoryHandler : IRequestHandler<GetAnimalWeightHistoryQuery, Result<IReadOnlyList<PortalWeightEntryDto>>>
{
    private readonly IOwnerAuthorizationService _ownerAuth;
    private readonly MedicalRecordsDbContext _context;

    public GetAnimalWeightHistoryHandler(IOwnerAuthorizationService ownerAuth, MedicalRecordsDbContext context)
    {
        _ownerAuth = ownerAuth;
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PortalWeightEntryDto>>> Handle(GetAnimalWeightHistoryQuery query, CancellationToken ct)
    {
        var authResult = await _ownerAuth.IsOwnerLinkedToPatient(query.OwnerAccountId, query.PatientId, ct);
        if (!authResult.IsSuccess)
            return Result<IReadOnlyList<PortalWeightEntryDto>>.Invalid(authResult.ValidationErrors.ToList());

        if (!authResult.Value)
            return Result<IReadOnlyList<PortalWeightEntryDto>>.Forbidden();

        var weights = await _context.WeightEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(w => w.PatientId == query.PatientId)
            .OrderByDescending(w => w.RecordedAt)
            .Select(w => new PortalWeightEntryDto(w.WeightKg, w.RecordedAt))
            .ToListAsync(ct);

        return Result<IReadOnlyList<PortalWeightEntryDto>>.Success(weights);
    }
}
