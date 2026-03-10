using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientReader : IPatientReader
{
    private readonly MedicalRecordsDbContext _context;

    public PatientReader(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PatientDto>>> GetPatientsByOwnerIdAsync(
        Guid ownerId,
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        // The global query filter already applies ClinicId = current tenant.
        // We additionally filter by OwnerId via the PatientOwner join table.
        var patients = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .Where(p => p.PatientOwners.Any(po => po.OwnerId == ownerId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = patients.Select(p => p.ToDto()).ToList();
        return Result<IReadOnlyList<PatientDto>>.Success(dtos);
    }
}
