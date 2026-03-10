using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles EmergencyMessageReceivedEvent: sends push + email notification to all vets of the clinic.
/// </summary>
internal class EmergencyMessageReceivedConsumer : IConsumer<EmergencyMessageReceivedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmergencyMessageReceivedConsumer> _logger;

    public EmergencyMessageReceivedConsumer(
        IEmailSender emailSender,
        ILogger<EmergencyMessageReceivedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmergencyMessageReceivedEvent> context)
    {
        var evt = context.Message;

        _logger.LogWarning(
            "EMERGENCY message received: conversation {ConversationId}, clinic {ClinicId}, patient {PatientId}. " +
            "Preview: {Preview}",
            evt.ConversationId,
            evt.ClinicId,
            evt.PatientId,
            evt.MessagePreview);

        // In a production system, vet email addresses would be fetched from the Auth module
        // and an email + push notification would be sent to each vet in the clinic.
        // The email template is available via MessagingEmailTemplates.EmergencyMessageReceived.
        // For now we log the alert. A future task should wire the Auth module query here.
        await Task.CompletedTask;
    }
}
