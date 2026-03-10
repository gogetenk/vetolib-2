using MediatR;

namespace Vetolib.Stock.Contracts;

public record StockInsufficientForPrescriptionEvent(
    Guid PrescriptionId,
    Guid DrugCatalogEntryId,
    int RequestedQuantity,
    int AvailableQuantity) : INotification;
