using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

/// <summary>
/// Handles SendMagicLinkEvent: sends the magic link email to the pet owner.
/// </summary>
internal class SendMagicLinkConsumer : IConsumer<SendMagicLinkEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendMagicLinkConsumer> _logger;

    public SendMagicLinkConsumer(IEmailSender emailSender, ILogger<SendMagicLinkConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendMagicLinkEvent> context)
    {
        var evt = context.Message;

        var message = new EmailMessage(
            To: evt.OwnerEmail,
            Subject: MessagingEmailTemplates.MagicLink.Subject(),
            HtmlBody: MessagingEmailTemplates.MagicLink.HtmlBody(evt.PortalUrl),
            PlainTextBody: MessagingEmailTemplates.MagicLink.PlainTextBody(evt.PortalUrl));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to send magic link email to {Email}: {Errors}",
                evt.OwnerEmail,
                string.Join(", ", result.Errors));

            // Intentional throw: MassTransit retry policy will requeue on transient failures
            throw new InvalidOperationException($"Failed to send magic link email to {evt.OwnerEmail}");
        }

        _logger.LogInformation("Magic link email sent to {Email} for owner {OwnerId}", evt.OwnerEmail, evt.OwnerId);
    }
}
