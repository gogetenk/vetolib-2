using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.AI.Application;
using Vetolib.AI.Application.Commands.GenerateHealthAlerts;

namespace Vetolib.AI.Infrastructure;

internal class HealthAlertGeneratorJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HealthAlertGeneratorJob> _logger;
    private readonly TimeSpan _scheduledTimeUtc;

    public HealthAlertGeneratorJob(
        IServiceScopeFactory scopeFactory,
        ILogger<HealthAlertGeneratorJob> logger,
        IOptions<HealthAlertJobOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _scheduledTimeUtc = options.Value.ScheduledTimeUtc;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "HealthAlertGeneratorJob started. Scheduled daily at {ScheduledTime} UTC",
            _scheduledTimeUtc);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = CalculateDelayUntilNextRun();
                _logger.LogDebug("Next health alert generation in {Delay}", delay);
                await Task.Delay(delay, stoppingToken);

                await RunGenerationAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in HealthAlertGeneratorJob. Will retry next cycle.");
                // Wait 1 minute before retrying to avoid tight error loops
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("HealthAlertGeneratorJob stopped");
    }

    private async Task RunGenerationAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting health alert generation");

        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GenerateHealthAlertsCommand(), stoppingToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Health alert generation completed. {AlertCount} new alerts generated", result.Value);
        }
        else
        {
            _logger.LogWarning("Health alert generation failed: {Errors}", string.Join(", ", result.Errors));
        }
    }

    private TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var todayRun = now.Date.Add(_scheduledTimeUtc);

        var nextRun = now < todayRun ? todayRun : todayRun.AddDays(1);
        return nextRun - now;
    }
}
