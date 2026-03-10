using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Queries.ListStockItems;

internal class ListStockItemsHandler : IRequestHandler<ListStockItemsQuery, Result<List<StockItemDto>>>
{
    private readonly StockDbContext _context;

    public ListStockItemsHandler(StockDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StockItemDto>>> Handle(ListStockItemsQuery query, CancellationToken ct)
    {
        var q = _context.StockItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Category) &&
            Enum.TryParse<StockItemCategory>(query.Category, ignoreCase: true, out var cat))
        {
            q = q.Where(x => x.Category == cat);
        }

        if (query.LowStock)
            q = q.Where(x => x.Quantity < x.MinThreshold);

        if (query.ExpiringSoon)
        {
            var threshold = DateTime.UtcNow.AddDays(30);
            q = q.Where(x => x.ExpiryDate != null && x.ExpiryDate <= threshold);
        }

        var items = await q.ToListAsync(ct);

        return Result<List<StockItemDto>>.Success(items.Select(i => i.ToDto()).ToList());
    }
}
