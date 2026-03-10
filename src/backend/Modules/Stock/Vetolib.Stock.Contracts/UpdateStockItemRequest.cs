namespace Vetolib.Stock.Contracts;

public record UpdateStockItemRequest(
    string? Name,
    int? MinThreshold
);
