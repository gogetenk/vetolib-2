using MediatR;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Stock.Application.Commands.DecrementStockForPrescription;
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
        if (!notification.StockDecrementConfirmed
            || notification.DrugCatalogEntryId is null
            || notification.Quantity is null)
        {
            return;
        }

        // Check availability first
        var availabilityResult = await _sender.Send(
            new CheckStockAvailabilityQuery(notification.DrugCatalogEntryId.Value, notification.ClinicId), ct);

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

        // Decrement stock for the prescription
        await _sender.Send(new DecrementStockByDrugCatalogEntryCommand(
            notification.DrugCatalogEntryId.Value,
            notification.Quantity.Value,
            notification.Id,
            notification.ClinicId), ct);
    }
}
