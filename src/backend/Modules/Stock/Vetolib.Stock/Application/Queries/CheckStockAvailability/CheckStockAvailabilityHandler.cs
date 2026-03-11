using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Queries.CheckStockAvailability;

internal class CheckStockAvailabilityHandler : IRequestHandler<CheckStockAvailabilityQuery, Result<StockAvailabilityResult>>
{
    private readonly StockDbContext _context;

    public CheckStockAvailabilityHandler(StockDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StockAvailabilityResult>> Handle(CheckStockAvailabilityQuery query, CancellationToken ct)
    {
        // Find stock items linked to this drug catalog entry
        var items = await _context.StockItems
            .AsNoTracking()
            .Where(s => s.DrugCatalogEntryId == query.DrugCatalogEntryId)
            .ToListAsync(ct);

        var primaryItem = items.FirstOrDefault(s => s.Quantity > 0)
            ?? items.FirstOrDefault();

        if (primaryItem is null || primaryItem.Quantity == 0)
        {
            // Out of stock — find alternatives in same category with available quantity
            var category = primaryItem?.Category;
            var alternatives = await BuildAlternatives(items.Select(s => s.Id).ToList(), category, ct);

            return Result<StockAvailabilityResult>.Success(new StockAvailabilityResult(
                Available: false,
                Quantity: primaryItem?.Quantity ?? 0,
                Unit: primaryItem?.Unit ?? string.Empty,
                IsLowStock: primaryItem?.IsLowStock ?? true,
                IsExpiringSoon: primaryItem?.IsExpiringSoon ?? false,
                Alternatives: alternatives,
                StockItemId: primaryItem?.Id));
        }

        return Result<StockAvailabilityResult>.Success(new StockAvailabilityResult(
            Available: true,
            Quantity: primaryItem.Quantity,
            Unit: primaryItem.Unit,
            IsLowStock: primaryItem.IsLowStock,
            IsExpiringSoon: primaryItem.IsExpiringSoon,
            Alternatives: [],
            StockItemId: primaryItem.Id));
    }

    private async Task<List<StockAlternativeDto>> BuildAlternatives(
        List<Guid> excludedIds,
        StockItemCategory? category,
        CancellationToken ct)
    {
        if (category is null)
            return [];

        var alternatives = await _context.StockItems
            .AsNoTracking()
            .Where(s => s.Category == category
                && s.Quantity > 0
                && !excludedIds.Contains(s.Id)
                && s.DrugCatalogEntryId != null)
            .Take(5)
            .ToListAsync(ct);

        return alternatives.Select(a => new StockAlternativeDto(
            a.Id,
            a.Name,
            a.DrugCatalogEntryId!.Value,
            a.Quantity,
            a.Unit)).ToList();
    }
}
