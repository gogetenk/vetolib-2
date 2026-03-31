using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalPrescriptions;

internal class GetAnimalPrescriptionsHandler : IRequestHandler<GetAnimalPrescriptionsQuery, Result<IReadOnlyList<PortalPrescriptionDto>>>
{
    private readonly IOwnerAuthorizationService _ownerAuth;
    private readonly MedicalRecordsDbContext _context;

    public GetAnimalPrescriptionsHandler(IOwnerAuthorizationService ownerAuth, MedicalRecordsDbContext context)
    {
        _ownerAuth = ownerAuth;
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PortalPrescriptionDto>>> Handle(GetAnimalPrescriptionsQuery query, CancellationToken ct)
    {
        var authResult = await _ownerAuth.IsOwnerLinkedToPatient(query.OwnerAccountId, query.PatientId, ct);
        if (!authResult.IsSuccess)
            return Result<IReadOnlyList<PortalPrescriptionDto>>.Invalid(authResult.ValidationErrors.ToList());

        if (!authResult.Value)
            return Result<IReadOnlyList<PortalPrescriptionDto>>.Forbidden();

        var prescriptions = await _context.MedicalRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId && r.IsVisibleToOwner)
            .SelectMany(r => r.Prescriptions.Select(p => new { Record = r, Prescription = p }))
            .OrderByDescending(x => x.Prescription.CreatedAt)
            .Select(x => new PortalPrescriptionDto(
                x.Prescription.Id,
                x.Prescription.Medication,
                x.Prescription.Dosage,
                x.Prescription.CreatedAt,
                x.Record.VetName))
            .ToListAsync(ct);

        return Result<IReadOnlyList<PortalPrescriptionDto>>.Success(prescriptions);
    }
}
