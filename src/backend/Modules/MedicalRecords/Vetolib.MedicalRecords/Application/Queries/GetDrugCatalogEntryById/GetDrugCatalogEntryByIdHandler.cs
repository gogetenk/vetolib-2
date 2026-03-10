using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetDrugCatalogEntryById;

internal class GetDrugCatalogEntryByIdHandler : IRequestHandler<GetDrugCatalogEntryByIdQuery, Result<DrugCatalogEntryDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetDrugCatalogEntryByIdHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DrugCatalogEntryDto>> Handle(
        GetDrugCatalogEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entry = await _context.DrugCatalogEntries
            .Include(d => d.SpeciesContraindications)
            .Include(d => d.Interactions)
            .Include(d => d.DosageGuidelines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entry is null)
            return Result<DrugCatalogEntryDto>.NotFound($"Drug catalog entry {request.Id} not found");

        return Result<DrugCatalogEntryDto>.Success(entry.ToDto());
    }
}
