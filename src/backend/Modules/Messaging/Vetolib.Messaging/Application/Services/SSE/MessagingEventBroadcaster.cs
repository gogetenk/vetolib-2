using System.Collections.Concurrent;
using System.Threading.Channels;
using Ardalis.Result;

namespace Vetolib.Messaging.Application.Services.SSE;

/// <summary>
/// Thread-safe singleton broadcaster. Each SSE connection gets its own bounded channel.
/// Events are filtered by clinicId and role before being written to the channel.
/// </summary>
internal sealed class MessagingEventBroadcaster : IMessagingEventBroadcaster
{
    private const int ChannelCapacity = 50;

    // Key: connectionId
    private readonly ConcurrentDictionary<string, SseConnection> _connections = new();

    // Tracks number of active SSE connections per clinic to prevent resource exhaustion
    private readonly ConcurrentDictionary<Guid, int> _clinicConnectionCounts = new();
    private readonly object _connectionLock = new();

    public Result<ChannelReader<MessagingEvent>> Subscribe(string connectionId, Guid clinicId, string role)
    {
        // Atomically check and increment the connection count for this clinic
        lock (_connectionLock)
        {
            var currentCount = _clinicConnectionCounts.GetValueOrDefault(clinicId, 0);
            if (currentCount >= IMessagingEventBroadcaster.MaxConnectionsPerClinic)
            {
                return Result<ChannelReader<MessagingEvent>>.Error(
                    $"Maximum SSE connections ({IMessagingEventBroadcaster.MaxConnectionsPerClinic}) reached for this clinic.");
            }

            _clinicConnectionCounts[clinicId] = currentCount + 1;
        }

        var channel = Channel.CreateBounded<MessagingEvent>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        var connection = new SseConnection(channel, clinicId, role);
        _connections[connectionId] = connection;
        return Result<ChannelReader<MessagingEvent>>.Success(channel.Reader);
    }

    public void Unsubscribe(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            connection.Channel.Writer.TryComplete();

            // Decrement the clinic connection count
            lock (_connectionLock)
            {
                if (_clinicConnectionCounts.TryGetValue(connection.ClinicId, out var count))
                {
                    if (count <= 1)
                        _clinicConnectionCounts.TryRemove(connection.ClinicId, out _);
                    else
                        _clinicConnectionCounts[connection.ClinicId] = count - 1;
                }
            }
        }
    }

    public async Task BroadcastAsync(MessagingEvent evt, CancellationToken ct = default)
    {
        foreach (var (_, connection) in _connections)
        {
            if (connection.ClinicId != evt.ClinicId)
                continue;

            // Receptionist role must not receive medical events
            if (IsReceptionist(connection.Role) && evt.IsMedical)
                continue;

            // Fire-and-forget per connection — don't let a slow consumer block others
            await connection.Channel.Writer.WriteAsync(evt, ct).ConfigureAwait(false);
        }
    }

    private static bool IsReceptionist(string role) =>
        string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase);

    private sealed record SseConnection(
        Channel<MessagingEvent> Channel,
        Guid ClinicId,
        string Role);
}
