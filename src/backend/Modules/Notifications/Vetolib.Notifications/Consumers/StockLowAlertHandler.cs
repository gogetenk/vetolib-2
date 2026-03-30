using MediatR;
using Microsoft.Extensions.Logging;
using Vetolib.Stock.Contracts;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles StockLowEvent (MediatR notification) published when stock quantity
/// drops below the configured minimum threshold after a stock movement.
/// Logs the alert for clinic awareness. Email notification requires an
/// IClinicStaffEmailReader (not yet available in Auth.Contracts).
/// </summary>
internal class StockLowAlertHandler : INotificationHandler<StockLowEvent>
{
    private readonly ILogger<StockLowAlertHandler> _logger;

    public StockLowAlertHandler(ILogger<StockLowAlertHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(StockLowEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "LOW STOCK ALERT — Item '{ItemName}' (ID: {StockItemId}) in clinic {ClinicId} " +
            "has {CurrentQuantity} units remaining (threshold: {MinThreshold}). " +
            "Please restock as soon as possible.",
            notification.StockItemName,
            notification.StockItemId,
            notification.ClinicId,
            notification.CurrentQuantity,
            notification.MinThreshold);

        // Once IClinicStaffEmailReader is available in Auth.Contracts,
        // resolve admin emails for the clinic and send using StockLowAlertEmailTemplate.
        // Pattern: see InvoiceSentConsumer for email sending with IEmailSender.

        return Task.CompletedTask;
    }
}
