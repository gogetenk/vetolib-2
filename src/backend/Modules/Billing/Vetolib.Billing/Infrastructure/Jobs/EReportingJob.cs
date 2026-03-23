using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.Billing.Application;
using Vetolib.Billing.Application.Commands.SubmitEReporting;

namespace Vetolib.Billing.Infrastructure.Jobs;

internal class EReportingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EReportingJob> _logger;
    private readonly EReportingJobOptions _options;

    public EReportingJob(
        IServiceScopeFactory scopeFactory,
        ILogger<EReportingJob> logger,
        IOptions<EReportingJobOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("E-reporting background job is disabled");
            return;
        }

        _logger.LogInformation("E-reporting background job started with interval {IntervalDays} days", _options.IntervalDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromDays(_options.IntervalDays), stoppingToken);
                await RunEReportingCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during e-reporting cycle");
            }
        }
    }

    internal async Task RunEReportingCycleAsync(CancellationToken ct)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodEnd = new DateOnly(now.Year, now.Month, 1).AddDays(-1); // Last day of previous month
        var periodStart = new DateOnly(periodEnd.Year, periodEnd.Month, 1); // First day of previous month

        _logger.LogInformation(
            "Running e-reporting for period {Start} to {End}",
            periodStart, periodEnd);

        using var scope = _scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new SubmitEReportingCommand(periodStart, periodEnd), ct);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "E-reporting submitted successfully for period {Start}-{End}, submission ID: {Id}",
                periodStart, periodEnd, result.Value.Id);
        }
        else
        {
            _logger.LogWarning(
                "E-reporting submission failed for period {Start}-{End}: {Errors}",
                periodStart, periodEnd, string.Join(", ", result.Errors));
        }
    }
}

internal class EReportingJobOptions
{
    public const string SectionName = "EReporting";

    /// <summary>
    /// Enable/disable the background e-reporting job. Default: false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Interval in days between e-reporting cycles. Default: 30 (monthly).
    /// </summary>
    public int IntervalDays { get; set; } = 30;
}
