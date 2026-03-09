using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class AppointmentReminderConsumer : IConsumer<AppointmentReminderDueIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentReminderConsumer> _logger;

    public AppointmentReminderConsumer(IEmailSender emailSender, ILogger<AppointmentReminderConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentReminderDueIntegrationEvent> context)
    {
        var evt = context.Message;
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

            // Throwing causes MassTransit to retry with exponential backoff
            throw new InvalidOperationException($"Failed to send reminder email to {evt.OwnerEmail}");
        }

        _logger.LogInformation("Reminder email sent to {Email}", evt.OwnerEmail);
    }
}
