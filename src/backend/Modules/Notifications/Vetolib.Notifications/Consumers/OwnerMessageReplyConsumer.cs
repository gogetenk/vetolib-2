using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles OwnerMessageReplyEvent: notifies the pet owner that a staff member has replied.
/// Note: OwnerEmail is not carried in this event (cross-module boundary).
/// Enrichment via Auth module query is the future path; for now we log and skip email dispatch.
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
            "Owner reply notification queued: conversation {ConversationId}, owner {OwnerId}, clinic {ClinicId}. " +
            "Preview: {Preview}. Email dispatch requires owner email enrichment via Auth module.",
            evt.ConversationId,
            evt.OwnerId,
            evt.ClinicId,
            evt.ReplyPreview);

        // Owner email is not carried in this event to avoid cross-module data coupling.
        // The owner portal URL would be: /portal/{clinicId}/conversations/{conversationId}
        // Future: subscribe to Auth module query to fetch OwnerEmail, then send email.
        return Task.CompletedTask;
    }
}
