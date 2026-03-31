using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalVaccinations;

internal class GetAnimalVaccinationsHandler : IRequestHandler<GetAnimalVaccinationsQuery, Result<IReadOnlyList<PortalVaccinationDto>>>
{
    private readonly IOwnerAuthorizationService _ownerAuth;
    private readonly MedicalRecordsDbContext _context;

    public GetAnimalVaccinationsHandler(IOwnerAuthorizationService ownerAuth, MedicalRecordsDbContext context)
    {
        _ownerAuth = ownerAuth;
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PortalVaccinationDto>>> Handle(GetAnimalVaccinationsQuery query, CancellationToken ct)
    {
        var authResult = await _ownerAuth.IsOwnerLinkedToPatient(query.OwnerAccountId, query.PatientId, ct);
        if (!authResult.IsSuccess)
            return Result<IReadOnlyList<PortalVaccinationDto>>.Invalid(authResult.ValidationErrors.ToList());

        if (!authResult.Value)
            return Result<IReadOnlyList<PortalVaccinationDto>>.Forbidden();

        // Vaccinations are prescriptions from visible medical records
        // that contain vaccination-related keywords in the diagnosis
        var vaccinations = await _context.MedicalRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId && r.IsVisibleToOwner)
            .SelectMany(r => r.Prescriptions.Select(p => new { Record = r, Prescription = p }))
            .OrderByDescending(x => x.Record.ExaminedAt)
            .Select(x => new PortalVaccinationDto(
                x.Prescription.Id,
                x.Prescription.Medication,
                x.Prescription.Dosage,
                x.Record.ExaminedAt,
                x.Record.VetName))
            .ToListAsync(ct);

        return Result<IReadOnlyList<PortalVaccinationDto>>.Success(vaccinations);
    }
}
