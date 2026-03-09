using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class UserInvitedConsumer : IConsumer<UserInvitedIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserInvitedConsumer> _logger;

    public UserInvitedConsumer(IEmailSender emailSender, ILogger<UserInvitedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserInvitedIntegrationEvent> context)
    {
        var evt = context.Message;

        var message = new EmailMessage(
            To: evt.Email,
            Subject: InvitationEmailTemplate.Subject(evt.ClinicName),
            HtmlBody: InvitationEmailTemplate.HtmlBody(evt.FullName, evt.Email, evt.TemporaryPassword, evt.ClinicName),
            PlainTextBody: InvitationEmailTemplate.PlainTextBody(evt.FullName, evt.Email, evt.TemporaryPassword, evt.ClinicName));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to send invitation email to {Email}: {Errors}",
                evt.Email,
                string.Join(", ", result.Errors));

            // Throwing causes MassTransit to retry with exponential backoff
            throw new InvalidOperationException($"Failed to send invitation email to {evt.Email}");
        }

        _logger.LogInformation("Invitation email sent to {Email}", evt.Email);
    }
}
