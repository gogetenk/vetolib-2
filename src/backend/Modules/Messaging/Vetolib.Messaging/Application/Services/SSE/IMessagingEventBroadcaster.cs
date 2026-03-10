using System.Threading.Channels;

namespace Vetolib.Messaging.Application.Services.SSE;

/// <summary>
/// Manages SSE connections and broadcasts messaging events to connected staff.
/// Registered as singleton.
/// </summary>
internal interface IMessagingEventBroadcaster
{
    /// <summary>
    /// Subscribes a new SSE connection and returns a channel reader for consuming events.
    /// The connection is identified by <paramref name="connectionId"/>.
    /// </summary>
    ChannelReader<MessagingEvent> Subscribe(string connectionId, Guid clinicId, string role);

    /// <summary>
    /// Removes the SSE connection identified by <paramref name="connectionId"/>.
    /// </summary>
    void Unsubscribe(string connectionId);

    /// <summary>
    /// Broadcasts an event to all connections matching the event's clinic and role filters.
    /// </summary>
    Task BroadcastAsync(MessagingEvent evt, CancellationToken ct = default);
}
