namespace Vetolib.Stock.Contracts;

public record CreateStockMovementRequest(
    string MovementType,
    int Quantity,
    string Reason
);
