using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles OwnerMessageReplyEvent: notifies the pet owner that a staff member has replied.
/// Note: owner email is not carried in this event. A future enrichment step should add
/// OwnerEmail to the event so the email can be dispatched directly here.
/// </summary>
internal class OwnerMessageReplyConsumer : IConsumer<OwnerMessageReplyEvent>
{
    private readonly ILogger<OwnerMessageReplyConsumer> _logger;

    public OwnerMessageReplyConsumer(ILogger<OwnerMessageReplyConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OwnerMessageReplyEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Owner reply notification: conversation {ConversationId}, owner {OwnerId}, clinic {ClinicId}. " +
            "Preview: {Preview}",
            evt.ConversationId,
            evt.OwnerId,
            evt.ClinicId,
            evt.ReplyPreview);

        // TODO: enrich with owner email from Auth module and send email notification.
        return Task.CompletedTask;
    }
}
