using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetMyAnimals;

internal class GetMyAnimalsHandler : IRequestHandler<GetMyAnimalsQuery, Result<IReadOnlyList<PortalAnimalDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetMyAnimalsHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PortalAnimalDto>>> Handle(GetMyAnimalsQuery query, CancellationToken ct)
    {
        if (query.OwnerAccountId == Guid.Empty)
            return Result<IReadOnlyList<PortalAnimalDto>>.Invalid(
                new ValidationError(nameof(query.OwnerAccountId), "OwnerAccountId is required"));

        // Cross-clinic query: IgnoreQueryFilters to see all clinics
        var animals = await _context.PatientOwners
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(po => po.Owner != null && po.Owner.OwnerAccountId == query.OwnerAccountId)
            .Select(po => po.Patient!)
            .Distinct()
            .Select(p => new PortalAnimalDto(
                p.Id,
                p.Name,
                p.Species,
                p.Breed,
                p.BirthDate,
                p.Sex,
                string.Empty, // ClinicName not available in MedicalRecords module
                p.ClinicId,
                p.MicrochipNumber))
            .ToListAsync(ct);

        return Result<IReadOnlyList<PortalAnimalDto>>.Success(animals);
    }
}
