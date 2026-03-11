using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class AppointmentReminderConsumer : IConsumer<AppointmentReminderDueIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IPreferenceChecker _preferenceChecker;
    private readonly ILogger<AppointmentReminderConsumer> _logger;

    public AppointmentReminderConsumer(
        IEmailSender emailSender,
        IPreferenceChecker preferenceChecker,
        ILogger<AppointmentReminderConsumer> logger)
    {
        _emailSender = emailSender;
        _preferenceChecker = preferenceChecker;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentReminderDueIntegrationEvent> context)
    {
        var evt = context.Message;

        // Preferences apply to clinic staff (Users) only, not to owners.
        // AppointmentReminderDueIntegrationEvent carries an OwnerEmail with no UserId —
        // this is an owner-facing email. Per PO decision, owners always receive reminders.
        // No preference check needed.

        var time = evt.ScheduledAt.ToString("HH:mm");

        var message = new EmailMessage(
            To: evt.OwnerEmail,
            Subject: ReminderEmailTemplate.Subject(evt.PatientName, time),
            HtmlBody: ReminderEmailTemplate.HtmlBody(evt.OwnerName, evt.PatientName, evt.VetName, evt.ScheduledAt),
            PlainTextBody: ReminderEmailTemplate.PlainTextBody(evt.OwnerName, evt.PatientName, evt.VetName, evt.ScheduledAt));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to send reminder email to {Email}: {Errors}",
                evt.OwnerEmail,
                string.Join(", ", result.Errors));

            // Intentional throw: MassTransit retry policy will requeue on transient failures
            throw new InvalidOperationException($"Failed to send reminder email to {evt.OwnerEmail}");
        }

        _logger.LogInformation("Reminder email sent to {Email}", evt.OwnerEmail);
    }
}
