using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Domain;
using Vetolib.Notifications.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class ReminderSchedulerServiceTests : IDisposable
{
    private readonly NotificationsDbContext _dbContext;
    private readonly ServiceProvider _serviceProvider;
    private readonly ReminderSchedulerService _service;

    public ReminderSchedulerServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        _dbContext = new NotificationsDbContext(options);

        var services = new ServiceCollection();
        services.AddSingleton(options);
        services.AddScoped(_ => new NotificationsDbContext(options));
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();

        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ReminderSchedulerService>.Instance;

        _service = new ReminderSchedulerService(scopeFactory, logger);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task ProcessReminders_WhenNoConfigsExist_DoesNotThrow()
    {
        var act = async () => await _service.ProcessRemindersAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessReminders_WithVaccinationConfigEnabled_CompletesSuccessfully()
    {
        var config = ReminderConfig.CreateDefault(
            new Guid("11111111-1111-1111-1111-111111111111")).Value;
        _dbContext.ReminderConfigs.Add(config);
        await _dbContext.SaveChangesAsync();

        var act = async () => await _service.ProcessRemindersAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessReminders_WithVaccinationDisabled_DoesNotCreateLogs()
    {
        var config = ReminderConfig.CreateDefault(
            new Guid("11111111-1111-1111-1111-111111111111")).Value;
        config.Update(true, false, true, 24, 7, Vetolib.Notifications.Contracts.Enums.ReminderChannel.Email);
        _dbContext.ReminderConfigs.Add(config);
        await _dbContext.SaveChangesAsync();

        await _service.ProcessRemindersAsync(CancellationToken.None);

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().BeEmpty();
    }

    [Fact]
    public void Interval_IsOneHour()
    {
        ReminderSchedulerService.Interval.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public async Task HasReminderBeenSent_WhenNoLogs_ReturnsFalse()
    {
        var result = await ReminderSchedulerService.HasReminderBeenSentAsync(
            _dbContext,
            ReminderType.Appointment24h,
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasReminderBeenSent_WhenLogExists_ReturnsTrue()
    {
        var appointmentId = Guid.NewGuid();
        var log = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            "ahmed@example.com",
            new Guid("11111111-1111-1111-1111-111111111111"),
            appointmentId: appointmentId).Value;
        log.MarkSent();
        _dbContext.ReminderLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        var result = await ReminderSchedulerService.HasReminderBeenSentAsync(
            _dbContext,
            ReminderType.Appointment24h,
            appointmentId,
            null,
            CancellationToken.None);

        result.Should().BeTrue();
    }
}
