using MediatR;

namespace Vetolib.Stock.Contracts;

public record StockLowEvent(
    Guid ClinicId,
    Guid StockItemId,
    string StockItemName,
    int CurrentQuantity,
    int MinThreshold
) : INotification;
