using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class UserInvitedConsumer : IConsumer<UserInvitedIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IPreferenceChecker _preferenceChecker;
    private readonly ILogger<UserInvitedConsumer> _logger;

    public UserInvitedConsumer(
        IEmailSender emailSender,
        IPreferenceChecker preferenceChecker,
        ILogger<UserInvitedConsumer> logger)
    {
        _emailSender = emailSender;
        _preferenceChecker = preferenceChecker;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserInvitedIntegrationEvent> context)
    {
        var evt = context.Message;

        // UserInvitedIntegrationEvent does not carry a UserId — the invited user
        // has not yet logged in and cannot have set preferences.
        // Per PO decision: no preference check when no UserId is available. Always send.

        var lang = evt.PreferredLanguage ?? "en";

        var message = new EmailMessage(
            To: evt.Email,
            Subject: InvitationEmailTemplate.Subject(evt.ClinicName, lang),
            HtmlBody: InvitationEmailTemplate.HtmlBody(evt.FullName, evt.Email, evt.TemporaryPassword, evt.ClinicName, lang),
            PlainTextBody: InvitationEmailTemplate.PlainTextBody(evt.FullName, evt.Email, evt.TemporaryPassword, evt.ClinicName, lang));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to send invitation email to {Email}: {Errors}",
                evt.Email,
                string.Join(", ", result.Errors));

            // Intentional throw: MassTransit retry policy will requeue on transient failures
            throw new InvalidOperationException($"Failed to send invitation email to {evt.Email}");
        }

        _logger.LogInformation("Invitation email sent to {Email}", evt.Email);
    }
}
