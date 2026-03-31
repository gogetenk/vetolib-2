using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class OwnerAccountLinker : IOwnerAccountLinker
{
    private readonly MedicalRecordsDbContext _context;

    public OwnerAccountLinker(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Guid[]> LinkOwnersByEmailOrPhoneAsync(Guid ownerAccountId, string email, string? phone, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedPhone = phone?.Trim();

        // Search across ALL clinics — IgnoreQueryFilters is justified here
        // because this is a cross-clinic identity operation during registration
        var matchingOwners = await _context.Owners
            .IgnoreQueryFilters()
            .Where(o => o.OwnerAccountId == null &&
                (o.Email == normalizedEmail ||
                 (normalizedPhone != null && o.Phone == normalizedPhone)))
            .ToListAsync(ct);

        var clinicIds = new HashSet<Guid>();

        foreach (var owner in matchingOwners)
        {
            owner.LinkToOwnerAccount(ownerAccountId);
            clinicIds.Add(owner.ClinicId);
        }

        if (matchingOwners.Count > 0)
            await _context.SaveChangesAsync(ct);

        return clinicIds.ToArray();
    }

    public async Task<Guid?> LinkOwnerByMicrochipAsync(Guid ownerAccountId, string microchipNumber, CancellationToken ct = default)
    {
        // Find patient with this microchip across ALL clinics
        var patient = await _context.Patients
            .IgnoreQueryFilters()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.MicrochipNumber == microchipNumber, ct);

        if (patient is null)
            return null;

        var linkedClinicId = (Guid?)null;

        foreach (var patientOwner in patient.PatientOwners)
        {
            if (patientOwner.Owner is not null && patientOwner.Owner.OwnerAccountId is null)
            {
                patientOwner.Owner.LinkToOwnerAccount(ownerAccountId);
                linkedClinicId = patientOwner.Owner.ClinicId;
            }
        }

        if (linkedClinicId is not null)
            await _context.SaveChangesAsync(ct);

        return linkedClinicId;
    }

    public async Task<Guid[]> GetLinkedClinicIdsAsync(Guid ownerAccountId, CancellationToken ct = default)
    {
        return await _context.Owners
            .IgnoreQueryFilters()
            .Where(o => o.OwnerAccountId == ownerAccountId)
            .Select(o => o.ClinicId)
            .Distinct()
            .ToArrayAsync(ct);
    }
}
