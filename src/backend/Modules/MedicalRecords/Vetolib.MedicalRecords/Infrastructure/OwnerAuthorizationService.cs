using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class OwnerAuthorizationService : IOwnerAuthorizationService
{
    private readonly MedicalRecordsDbContext _context;

    public OwnerAuthorizationService(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> IsOwnerLinkedToPatient(Guid ownerAccountId, Guid patientId, CancellationToken ct)
    {
        if (ownerAccountId == Guid.Empty)
            return Result<bool>.Invalid(new ValidationError(nameof(ownerAccountId), "OwnerAccountId is required"));

        if (patientId == Guid.Empty)
            return Result<bool>.Invalid(new ValidationError(nameof(patientId), "PatientId is required"));

        // Portal queries cross clinics, so we need IgnoreQueryFilters
        var isLinked = await _context.PatientOwners
            .IgnoreQueryFilters()
            .AnyAsync(po =>
                po.PatientId == patientId &&
                po.Owner != null &&
                po.Owner.OwnerAccountId == ownerAccountId, ct);

        return Result<bool>.Success(isLinked);
    }
}
