using Ardalis.Result;

namespace Vetolib.Messaging.Contracts;

public interface IChannelDispatcher
{
    Task<Result> SendAsync(ChannelMessage message, CancellationToken ct = default);
}

public record ChannelMessage(
    string RecipientPhone,
    string TemplateName,
    Dictionary<string, string> Parameters,
    Guid ClinicId);
