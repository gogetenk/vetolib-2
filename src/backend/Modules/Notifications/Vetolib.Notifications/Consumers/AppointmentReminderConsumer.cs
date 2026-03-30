using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Domain;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Notifications.Templates;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class AppointmentReminderConsumer : IConsumer<AppointmentReminderDueIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IPreferenceChecker _preferenceChecker;
    private readonly NotificationsDbContext _dbContext;
    private readonly ILogger<AppointmentReminderConsumer> _logger;

    public AppointmentReminderConsumer(
        IEmailSender emailSender,
        IPreferenceChecker preferenceChecker,
        NotificationsDbContext dbContext,
        ILogger<AppointmentReminderConsumer> logger)
    {
        _emailSender = emailSender;
        _preferenceChecker = preferenceChecker;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentReminderDueIntegrationEvent> context)
    {
        var evt = context.Message;

        // Preferences apply to clinic staff (Users) only, not to owners.
        // AppointmentReminderDueIntegrationEvent carries an OwnerEmail with no UserId —
        // this is an owner-facing email. Per PO decision, owners always receive reminders.
        // No preference check needed.

        var lang = evt.PreferredLanguage ?? "en";
        var time = evt.ScheduledAt.ToString("HH:mm");

        var message = new EmailMessage(
            To: evt.OwnerEmail,
            Subject: ReminderEmailTemplate.Subject(evt.PatientName, time, lang),
            HtmlBody: ReminderEmailTemplate.HtmlBody(evt.OwnerName, evt.PatientName, evt.VetName, evt.ScheduledAt, lang),
            PlainTextBody: ReminderEmailTemplate.PlainTextBody(evt.OwnerName, evt.PatientName, evt.VetName, evt.ScheduledAt, lang));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        // Log the reminder attempt
        var logResult = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            evt.OwnerEmail,
            Guid.Empty, // ClinicId not available in the event — logged for audit
            appointmentId: null);

        if (logResult.IsSuccess)
        {
            var log = logResult.Value;
            if (result.IsSuccess) log.MarkSent(); else log.MarkFailed();
            _dbContext.ReminderLogs.Add(log);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }

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
