using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Contracts.Events;
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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly NotificationsDbContext _dbContext;
    private readonly ILogger<AppointmentReminderConsumer> _logger;

    public AppointmentReminderConsumer(
        IEmailSender emailSender,
        IPreferenceChecker preferenceChecker,
        IPublishEndpoint publishEndpoint,
        NotificationsDbContext dbContext,
        ILogger<AppointmentReminderConsumer> logger)
    {
        _emailSender = emailSender;
        _preferenceChecker = preferenceChecker;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentReminderDueIntegrationEvent> context)
    {
        var evt = context.Message;

        var channel = await ResolveChannelAsync(evt.ClinicId, context.CancellationToken);

        var shouldSendEmail = channel is ReminderChannel.Email or ReminderChannel.Both;
        var shouldSendWhatsApp = channel is ReminderChannel.WhatsApp or ReminderChannel.Both;

        if (shouldSendEmail)
        {
            await SendEmailReminderAsync(evt, context.CancellationToken);
        }

        if (shouldSendWhatsApp)
        {
            await SendWhatsAppReminderAsync(evt, context.CancellationToken);
        }
    }

    internal async Task<ReminderChannel> ResolveChannelAsync(Guid clinicId, CancellationToken ct)
    {
        if (clinicId == Guid.Empty)
            return ReminderChannel.Email;

        var config = await _dbContext.ReminderConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ClinicId == clinicId, ct);

        return config?.PreferredReminderChannel ?? ReminderChannel.Email;
    }

    private async Task SendEmailReminderAsync(AppointmentReminderDueIntegrationEvent evt, CancellationToken ct)
    {
        var lang = evt.PreferredLanguage ?? "en";
        var time = evt.ScheduledAt.ToString("HH:mm");

        var message = new EmailMessage(
            To: evt.OwnerEmail,
            Subject: ReminderEmailTemplate.Subject(evt.PatientName, time, lang),
            HtmlBody: ReminderEmailTemplate.HtmlBody(evt.OwnerName, evt.PatientName, evt.VetName, evt.ScheduledAt, lang),
            PlainTextBody: ReminderEmailTemplate.PlainTextBody(evt.OwnerName, evt.PatientName, evt.VetName, evt.ScheduledAt, lang));

        var result = await _emailSender.SendAsync(message, ct);

        var logResult = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            evt.OwnerEmail,
            evt.ClinicId,
            appointmentId: null);

        if (logResult.IsSuccess)
        {
            var log = logResult.Value;
            if (result.IsSuccess) log.MarkSent(); else log.MarkFailed();
            _dbContext.ReminderLogs.Add(log);
            await _dbContext.SaveChangesAsync(ct);
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

    private async Task SendWhatsAppReminderAsync(AppointmentReminderDueIntegrationEvent evt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(evt.OwnerPhone))
        {
            _logger.LogWarning(
                "WhatsApp reminder skipped for {Email} — no phone number available.",
                evt.OwnerEmail);
            return;
        }

        await _publishEndpoint.Publish(new SendWhatsAppReminderEvent
        {
            OwnerPhone = evt.OwnerPhone,
            OwnerName = evt.OwnerName,
            PatientName = evt.PatientName,
            VetName = evt.VetName,
            ScheduledAt = evt.ScheduledAt,
            ClinicName = evt.ClinicName,
            PreferredLanguage = evt.PreferredLanguage
        }, ct);

        // Log the WhatsApp reminder attempt (delivery tracked by Messaging module)
        var logResult = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Sms, // Using Sms channel for WhatsApp in logs (closest existing enum value)
            evt.OwnerPhone,
            evt.ClinicId,
            appointmentId: null);

        if (logResult.IsSuccess)
        {
            var log = logResult.Value;
            log.MarkSent(); // Mark as sent — actual delivery tracked by Messaging module
            _dbContext.ReminderLogs.Add(log);
            await _dbContext.SaveChangesAsync(ct);
        }

        _logger.LogInformation("WhatsApp reminder event published for {Phone}", evt.OwnerPhone);
    }
}
