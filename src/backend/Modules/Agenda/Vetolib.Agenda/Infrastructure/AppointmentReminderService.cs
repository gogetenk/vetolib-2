using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Infrastructure;

internal class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public AppointmentReminderService(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentReminderService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing appointment reminders.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        // We bypass tenant filter for this background service
        // because it processes appointments across all clinics.
        var dbContext = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var now = DateTime.UtcNow;
        var windowStart = now.AddHours(23);
        var windowEnd = now.AddHours(25);

        var dateStart = DateOnly.FromDateTime(windowStart);
        var dateEnd = DateOnly.FromDateTime(windowEnd);
        var timeStart = TimeOnly.FromDateTime(windowStart);
        var timeEnd = TimeOnly.FromDateTime(windowEnd);

        // Query appointments in the 23–25h window that have not been reminded yet
        var appointments = await dbContext.Appointments
            .IgnoreQueryFilters()
            .Where(a =>
                !a.ReminderSent &&
                a.Status == AppointmentStatus.Scheduled &&
                a.OwnerEmail != null &&
                ((a.Date == dateStart && a.StartTime >= timeStart) ||
                 (a.Date == dateEnd && a.StartTime <= timeEnd) ||
                 (dateStart < dateEnd && a.Date > dateStart && a.Date < dateEnd)))
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} appointments to remind.", appointments.Count);

        foreach (var appointment in appointments)
        {
            // Double-check the appointment falls in the 23–25h window
            var scheduledAt = appointment.Date.ToDateTime(appointment.StartTime);
            var hoursUntil = (scheduledAt - now).TotalHours;

            if (hoursUntil < 23 || hoursUntil > 25)
                continue;

            // Publish integration event — Notifications module sends the email
            await publishEndpoint.Publish(new AppointmentReminderDueIntegrationEvent
            {
                OwnerEmail = appointment.OwnerEmail!,
                OwnerName = appointment.OwnerName,
                PatientName = appointment.AnimalName,
                VetName = appointment.VeterinarianName,
                ScheduledAt = scheduledAt,
                ClinicName = "Desert Paws Veterinary Clinic"
            }, ct);

            appointment.MarkReminderSent();

            _logger.LogInformation(
                "Reminder event published for appointment {Id} to {Email}",
                appointment.Id,
                appointment.OwnerEmail);
        }

        if (appointments.Any())
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
