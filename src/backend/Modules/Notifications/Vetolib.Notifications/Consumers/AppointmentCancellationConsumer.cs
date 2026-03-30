using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class AppointmentCancellationConsumer : IConsumer<AppointmentCancelledIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentCancellationConsumer> _logger;

    public AppointmentCancellationConsumer(
        IEmailSender emailSender,
        ILogger<AppointmentCancellationConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentCancelledIntegrationEvent> context)
    {
        var evt = context.Message;
        var lang = evt.PreferredLanguage ?? "en";

        var message = new EmailMessage(
            To: evt.OwnerEmail,
            Subject: CancellationEmailTemplate.Subject(evt.PatientName, lang),
            HtmlBody: CancellationEmailTemplate.HtmlBody(
                evt.OwnerName, evt.PatientName, evt.VetName,
                evt.ScheduledAt, evt.ClinicName, evt.CancellationReason, lang),
            PlainTextBody: CancellationEmailTemplate.PlainTextBody(
                evt.OwnerName, evt.PatientName, evt.VetName,
                evt.ScheduledAt, evt.ClinicName, evt.CancellationReason, lang));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to send cancellation email to {Email}: {Errors}",
                evt.OwnerEmail,
                string.Join(", ", result.Errors));

            // Intentional throw: MassTransit retry policy will requeue on transient failures
            throw new InvalidOperationException($"Failed to send cancellation email to {evt.OwnerEmail}");
        }

        _logger.LogInformation("Cancellation email sent to {Email} for {Patient}", evt.OwnerEmail, evt.PatientName);
    }
}
