using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.ScheduleFollowUp;

internal class AppointmentCompletedEventHandler : INotificationHandler<AppointmentCompletedEvent>
{
    private readonly AgendaDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AppointmentCompletedEventHandler> _logger;

    public AppointmentCompletedEventHandler(
        AgendaDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<AppointmentCompletedEventHandler> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(AppointmentCompletedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.Reason))
        {
            _logger.LogDebug(
                "Appointment {AppointmentId} completed without a reason — skipping follow-up check",
                notification.AppointmentId);
            return;
        }

        var matchingRule = await _context.FollowUpRules
            .Where(r => r.IsActive)
            .FirstOrDefaultAsync(r => r.ConsultationType == notification.Reason, ct);

        if (matchingRule is null)
        {
            _logger.LogDebug(
                "No active follow-up rule matches reason '{Reason}' for appointment {AppointmentId}",
                notification.Reason, notification.AppointmentId);
            return;
        }

        var followUpDate = notification.Date.AddDays(matchingRule.FollowUpDays);
        var defaultStartTime = new TimeOnly(9, 0); // Default 9:00 AM slot
        const int defaultDurationMinutes = 30;

        var createResult = Appointment.Create(
            notification.ClinicId,
            notification.VeterinarianId,
            notification.VeterinarianName,
            notification.AnimalId,
            notification.AnimalName,
            notification.OwnerName,
            notification.OwnerEmail,
            followUpDate,
            defaultStartTime,
            defaultDurationMinutes,
            matchingRule.FollowUpReason,
            BookingSource.System);

        if (!createResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to create follow-up appointment for {AppointmentId}: {Errors}",
                notification.AppointmentId,
                string.Join("; ", createResult.Errors));
            return;
        }

        _context.Appointments.Add(createResult.Value);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Scheduled follow-up appointment {FollowUpId} on {FollowUpDate} for completed appointment {AppointmentId} (rule: {Rule})",
            createResult.Value.Id, followUpDate, notification.AppointmentId, matchingRule.ConsultationType);

        // Notify owner via the notification system
        if (!string.IsNullOrWhiteSpace(notification.OwnerEmail))
        {
            await _publishEndpoint.Publish(new FollowUpScheduledIntegrationEvent
            {
                OwnerEmail = notification.OwnerEmail,
                OwnerName = notification.OwnerName,
                PatientName = notification.AnimalName,
                VetName = notification.VeterinarianName,
                FollowUpDate = followUpDate.ToDateTime(defaultStartTime),
                FollowUpReason = matchingRule.FollowUpReason,
                ClinicId = notification.ClinicId
            }, ct);
        }
    }
}
