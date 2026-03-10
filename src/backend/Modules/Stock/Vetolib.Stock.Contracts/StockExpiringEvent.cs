using MediatR;

namespace Vetolib.Stock.Contracts;

public record StockExpiringEvent(
    Guid ClinicId,
    Guid StockItemId,
    string StockItemName,
    DateTime ExpiryDate
) : INotification;
