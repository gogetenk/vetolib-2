namespace Vetolib.Stock.Contracts;

public record StockMovementDto(
    Guid Id,
    Guid StockItemId,
    string MovementType,
    int Quantity,
    string Reason,
    string CreatedBy,
    DateTime CreatedAt
);
