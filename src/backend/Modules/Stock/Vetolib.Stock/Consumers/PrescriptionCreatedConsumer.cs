using MediatR;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Consumers;

internal class PrescriptionCreatedConsumer : INotificationHandler<PrescriptionCreatedEvent>
{
    private readonly ISender _sender;
    private readonly IPublisher _publisher;

    public PrescriptionCreatedConsumer(ISender sender, IPublisher publisher)
    {
        _sender = sender;
        _publisher = publisher;
    }

    public async Task Handle(PrescriptionCreatedEvent notification, CancellationToken ct)
    {
        if (!notification.StockDecrementConfirmed || notification.DrugCatalogEntryId is null || notification.Quantity is null)
            return;

        // Find the stock item linked to this drug catalog entry
        var availabilityQuery = new CheckStockAvailabilityQuery(
            notification.DrugCatalogEntryId.Value,
            notification.ClinicId);

        var availabilityResult = await _sender.Send(availabilityQuery, ct);
        if (!availabilityResult.IsSuccess)
            return;

        var availability = availabilityResult.Value;

        if (!availability.Available || availability.Quantity < notification.Quantity.Value)
        {
            await _publisher.Publish(new StockInsufficientForPrescriptionEvent(
                notification.Id,
                notification.DrugCatalogEntryId.Value,
                notification.Quantity.Value,
                availability.Quantity), ct);
            return;
        }

        // Find the specific stock item id to decrement
        // We need to resolve the actual StockItemId from the availability check
        // The CheckStockAvailabilityQuery doesn't return the StockItemId directly,
        // so we send a DecrementStockForPrescriptionCommand via a targeted query approach.
        // Since CheckStockAvailabilityQuery returns the primary item's data but not its Id,
        // we use a dedicated internal lookup here.
        var decrementCommand = new DecrementStockByDrugCatalogEntryCommand(
            notification.DrugCatalogEntryId.Value,
            notification.Quantity.Value,
            notification.Id,
            notification.ClinicId);

        await _sender.Send(decrementCommand, ct);
    }
}
