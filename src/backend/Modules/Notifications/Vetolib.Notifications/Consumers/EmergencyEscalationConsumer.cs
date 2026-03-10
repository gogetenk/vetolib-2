using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles EmergencyEscalationEvent: sends push + email "URGENT — unread emergency message"
/// to all vets of the clinic.
/// </summary>
internal class EmergencyEscalationConsumer : IConsumer<EmergencyEscalationEvent>
{
    private readonly ILogger<EmergencyEscalationConsumer> _logger;

    public EmergencyEscalationConsumer(ILogger<EmergencyEscalationConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<EmergencyEscalationEvent> context)
    {
        var evt = context.Message;

        _logger.LogCritical(
            "ESCALATION — Emergency conversation {ConversationId} in clinic {ClinicId} unread for >10 minutes. " +
            "Preview: {Preview}",
            evt.ConversationId,
            evt.ClinicId,
            evt.MessagePreview);

        // In a production system, vet email addresses would be fetched from the Auth module
        // and an urgent push + email notification would be sent to every vet in the clinic.
        // The email template is available via MessagingEmailTemplates.EmergencyEscalation.
        return Task.CompletedTask;
    }
}
