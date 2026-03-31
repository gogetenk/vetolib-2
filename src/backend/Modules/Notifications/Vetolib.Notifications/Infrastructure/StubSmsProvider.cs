using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Vetolib.Notifications.Contracts;

namespace Vetolib.Notifications.Infrastructure;

/// <summary>
/// Stub SMS provider that logs messages instead of sending them.
/// Replace with TwilioSmsProvider when the Twilio account is provisioned.
/// </summary>
internal class StubSmsProvider : ISmsProvider
{
    private readonly ILogger<StubSmsProvider> _logger;

    public StubSmsProvider(ILogger<StubSmsProvider> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("SMS stub: {Phone} -> {Message}", phoneNumber, message);
        return Task.FromResult(Result.Success());
    }
}
