using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles OutboundConversationCreatedEvent: notifies the pet owner that the clinic
/// has started a new conversation with them.
/// </summary>
internal class OutboundConversationCreatedConsumer : IConsumer<OutboundConversationCreatedEvent>
{
    private readonly ILogger<OutboundConversationCreatedConsumer> _logger;

    public OutboundConversationCreatedConsumer(ILogger<OutboundConversationCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OutboundConversationCreatedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Outbound conversation created: conversation {ConversationId}, owner {OwnerId}, clinic {ClinicId}. " +
            "Preview: {Preview}",
            evt.ConversationId,
            evt.OwnerId,
            evt.ClinicId,
            evt.MessagePreview);

        // In a production system, the owner's email would be fetched from the Auth module
        // and an email notification would be sent with a link to the owner portal.
        // The email template is available via MessagingEmailTemplates.OutboundConversation.
        return Task.CompletedTask;
    }
}
