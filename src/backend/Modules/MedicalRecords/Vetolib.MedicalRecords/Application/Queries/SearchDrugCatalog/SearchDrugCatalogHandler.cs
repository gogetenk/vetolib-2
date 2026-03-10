using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.SearchDrugCatalog;

internal class SearchDrugCatalogHandler : IRequestHandler<SearchDrugCatalogQuery, Result<List<DrugCatalogEntryDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public SearchDrugCatalogHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DrugCatalogEntryDto>>> Handle(
        SearchDrugCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var limit = request.Limit is > 0 and <= 100 ? request.Limit : 20;

        var query = _context.DrugCatalogEntries
            .Include(d => d.SpeciesContraindications)
            .Include(d => d.Interactions)
            .Include(d => d.DosageGuidelines)
            .Where(d => d.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower().Trim();
            query = query.Where(d =>
                d.InnName.ToLower().Contains(term) ||
                d.DisplayName.ToLower().Contains(term));
        }

        var entries = await query
            .OrderBy(d => d.InnName)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Result<List<DrugCatalogEntryDto>>.Success(
            entries.Select(e => e.ToDto()).ToList());
    }
}
