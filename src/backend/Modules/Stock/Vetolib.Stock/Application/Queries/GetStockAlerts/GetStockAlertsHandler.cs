using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Queries.GetStockAlerts;

internal class GetStockAlertsHandler : IRequestHandler<GetStockAlertsQuery, Result<StockAlertsDto>>
{
    private readonly StockDbContext _context;
    private readonly IPublisher _publisher;
    private readonly StockOptions _options;

    public GetStockAlertsHandler(StockDbContext context, IPublisher publisher, IOptions<StockOptions> options)
    {
        _context = context;
        _publisher = publisher;
        _options = options.Value;
    }

    public async Task<Result<StockAlertsDto>> Handle(GetStockAlertsQuery query, CancellationToken ct)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(_options.ExpiryWarningDays);

        var lowStockItems = await _context.StockItems
            .AsNoTracking()
            .Where(i => i.Quantity < i.MinThreshold)
            .ToListAsync(ct);

        var expiringItems = await _context.StockItems
            .AsNoTracking()
            .Where(i => i.ExpiryDate != null && i.ExpiryDate <= expiryThreshold)
            .ToListAsync(ct);

        foreach (var item in expiringItems)
        {
            await _publisher.Publish(new StockExpiringEvent(
                item.ClinicId,
                item.Id,
                item.Name,
                item.ExpiryDate!.Value), ct);
        }

        return Result<StockAlertsDto>.Success(new StockAlertsDto(
            lowStockItems.Select(i => i.ToDto(_options.ExpiryWarningDays)).ToList(),
            expiringItems.Select(i => i.ToDto(_options.ExpiryWarningDays)).ToList()));
    }
}
