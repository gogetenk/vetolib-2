using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Vetolib.Preferences.Application.Services;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Consumers;

/// <summary>
/// Invalidates the IMemoryCache entry when a preference changes.
/// Ensures that the PreferenceChecker reflects updates within 5 minutes at most,
/// or immediately when this consumer processes the event.
/// </summary>
internal class PreferenceChangedConsumer : IConsumer<PreferenceChangedIntegrationEvent>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<PreferenceChangedConsumer> _logger;

    public PreferenceChangedConsumer(IMemoryCache cache, ILogger<PreferenceChangedConsumer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PreferenceChangedIntegrationEvent> context)
    {
        var evt = context.Message;
        var cacheKey = PreferenceChecker.BuildCacheKey(evt.ClinicId, evt.UserId, evt.Key);
        _cache.Remove(cacheKey);

        _logger.LogInformation(
            "Preference cache invalidated for ClinicId={ClinicId}, UserId={UserId}, Key={Key}",
            evt.ClinicId,
            evt.UserId,
            evt.Key);

        return Task.CompletedTask;
    }
}
