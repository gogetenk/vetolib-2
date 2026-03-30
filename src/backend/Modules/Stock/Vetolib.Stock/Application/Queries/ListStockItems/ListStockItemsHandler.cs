using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Queries.ListStockItems;

internal class ListStockItemsHandler : IRequestHandler<ListStockItemsQuery, Result<List<StockItemDto>>>
{
    private readonly StockDbContext _context;
    private readonly StockOptions _options;

    public ListStockItemsHandler(StockDbContext context, IOptions<StockOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Result<List<StockItemDto>>> Handle(ListStockItemsQuery query, CancellationToken ct)
    {
        var q = _context.StockItems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Category) &&
            Enum.TryParse<StockItemCategory>(query.Category, ignoreCase: true, out var cat))
        {
            q = q.Where(x => x.Category == cat);
        }

        if (query.LowStock)
            q = q.Where(x => x.Quantity < x.MinThreshold);

        if (query.ExpiringSoon)
        {
            var threshold = DateTime.UtcNow.AddDays(_options.ExpiryWarningDays);
            q = q.Where(x => x.ExpiryDate != null && x.ExpiryDate <= threshold);
        }

        // P-10: Apply pagination to prevent unbounded result sets
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await q
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Result<List<StockItemDto>>.Success(items.Select(i => i.ToDto(_options.ExpiryWarningDays)).ToList());
    }
}
