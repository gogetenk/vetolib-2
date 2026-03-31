using Ardalis.Result;

namespace Vetolib.Notifications.Contracts;

/// <summary>
/// Abstraction for sending SMS messages.
/// Current implementation is a stub logger; swap for Twilio when ready.
/// </summary>
public interface ISmsProvider
{
    Task<Result> SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default);
}
