namespace Vetolib.Stock.Contracts;

public record StockItemDto(
    Guid Id,
    Guid ClinicId,
    string Name,
    string Category,
    int Quantity,
    string Unit,
    int MinThreshold,
    DateTime? ExpiryDate,
    bool IsLowStock,
    bool IsExpiringSoon,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
