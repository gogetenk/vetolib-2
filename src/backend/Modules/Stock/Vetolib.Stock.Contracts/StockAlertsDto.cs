namespace Vetolib.Stock.Contracts;

public record StockAlertsDto(
    List<StockItemDto> LowStockItems,
    List<StockItemDto> ExpiringItems
);
