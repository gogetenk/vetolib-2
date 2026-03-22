using MediatR;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;

namespace Vetolib.Messaging.Application.Consumers;

internal class UrgentMessageClassifiedEventConsumer : INotificationHandler<UrgentMessageClassifiedEvent>
{
    private readonly ILogger<UrgentMessageClassifiedEventConsumer> _logger;

    public UrgentMessageClassifiedEventConsumer(ILogger<UrgentMessageClassifiedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(UrgentMessageClassifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "URGENT message classified — MessageId={MessageId}, ConversationId={ConversationId}, " +
            "ClinicId={ClinicId}, Urgency={Urgency}, Preview=\"{Preview}\"",
            notification.MessageId,
            notification.ConversationId,
            notification.ClinicId,
            notification.Urgency,
            notification.MessagePreview);

        // Future: push notification to vet on-call, SMS alert, etc.

        return Task.CompletedTask;
    }
}
