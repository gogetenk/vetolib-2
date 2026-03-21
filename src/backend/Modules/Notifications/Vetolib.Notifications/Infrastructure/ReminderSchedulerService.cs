using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Domain;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Infrastructure;

/// <summary>
/// Background service that runs every hour to detect and send reminders
/// for overdue vaccinations and follow-ups.
/// Appointment 24h reminders are handled by the Agenda module's AppointmentReminderService
/// which publishes AppointmentReminderDueIntegrationEvent consumed by AppointmentReminderConsumer.
/// This service focuses on vaccination and follow-up reminders that require cross-module queries.
/// </summary>
internal class ReminderSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderSchedulerService> _logger;
    internal static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public ReminderSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReminderSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderSchedulerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing scheduled reminders.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    internal async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        await ProcessVaccinationRemindersAsync(dbContext, ct);
    }

    private async Task ProcessVaccinationRemindersAsync(
        NotificationsDbContext dbContext,
        CancellationToken ct)
    {
        // Get all configs that have vaccination reminders enabled
        // EXCEPTION approved: cross-tenant background service scans all clinics
        var configs = await dbContext.ReminderConfigs
            .IgnoreQueryFilters()
            .Where(c => c.VaccinationDueEnabled)
            .ToListAsync(ct);

        if (configs.Count == 0)
        {
            _logger.LogDebug("No clinics have vaccination reminders enabled.");
            return;
        }

        // Vaccination reminders are based on patient medical records.
        // Since there is no dedicated vaccination tracking entity with NextDueDate in MedicalRecords yet,
        // this is a placeholder that logs the check.
        // When MedicalRecords exposes an IVaccinationReader contract with overdue vaccination queries,
        // this method will query overdue vaccinations and send reminders.
        _logger.LogInformation(
            "Vaccination reminder check completed. {ConfigCount} clinic(s) have vaccination reminders enabled.",
            configs.Count);
    }

    /// <summary>
    /// Checks whether a reminder of the given type has already been sent for the specified target.
    /// Used by consumers to prevent duplicate reminders.
    /// </summary>
    internal static async Task<bool> HasReminderBeenSentAsync(
        NotificationsDbContext dbContext,
        ReminderType reminderType,
        Guid? appointmentId,
        Guid? patientId,
        CancellationToken ct)
    {
        // EXCEPTION approved: cross-tenant — reminder dedup needs global scan
        return await dbContext.ReminderLogs
            .IgnoreQueryFilters()
            .AnyAsync(r =>
                r.ReminderType == reminderType &&
                (appointmentId.HasValue && r.AppointmentId == appointmentId) ||
                (patientId.HasValue && r.PatientId == patientId), ct);
    }
}
