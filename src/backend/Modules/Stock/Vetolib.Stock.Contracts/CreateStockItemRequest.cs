namespace Vetolib.Stock.Contracts;

public record CreateStockItemRequest(
    string Name,
    string Category,
    int Quantity,
    string Unit,
    int MinThreshold,
    DateTime? ExpiryDate,
    Guid? DrugCatalogEntryId = null
);
