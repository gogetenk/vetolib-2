using Ardalis.Result;
using FluentAssertions;
using Vetolib.Messaging.Application.Services.SSE;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class MessagingEventBroadcasterTests
{
    private readonly MessagingEventBroadcaster _sut = new();
    private readonly Guid _clinicId = Guid.NewGuid();

    [Fact]
    public void Subscribe_WhenUnderLimit_ReturnsSuccess()
    {
        var result = _sut.Subscribe("conn-1", _clinicId, "Vet");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Subscribe_WhenAtLimit_ReturnsError()
    {
        // Fill up to the max
        for (var i = 0; i < IMessagingEventBroadcaster.MaxConnectionsPerClinic; i++)
        {
            var r = _sut.Subscribe($"conn-{i}", _clinicId, "Vet");
            r.IsSuccess.Should().BeTrue($"connection {i} should succeed");
        }

        // The next one should be rejected
        var result = _sut.Subscribe("conn-overflow", _clinicId, "Vet");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("Maximum SSE connections"));
    }

    [Fact]
    public void Subscribe_DifferentClinics_HaveIndependentLimits()
    {
        var otherClinicId = Guid.NewGuid();

        // Fill up clinic A
        for (var i = 0; i < IMessagingEventBroadcaster.MaxConnectionsPerClinic; i++)
            _sut.Subscribe($"conn-a-{i}", _clinicId, "Vet");

        // Clinic B should still accept connections
        var result = _sut.Subscribe("conn-b-0", otherClinicId, "Vet");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Unsubscribe_FreesSlot_AllowsNewConnection()
    {
        // Fill up to the max
        for (var i = 0; i < IMessagingEventBroadcaster.MaxConnectionsPerClinic; i++)
            _sut.Subscribe($"conn-{i}", _clinicId, "Vet");

        // Verify full
        _sut.Subscribe("conn-overflow", _clinicId, "Vet").IsSuccess.Should().BeFalse();

        // Unsubscribe one
        _sut.Unsubscribe("conn-0");

        // Now a new connection should succeed
        var result = _sut.Subscribe("conn-new", _clinicId, "Vet");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Unsubscribe_UnknownConnectionId_DoesNotThrow()
    {
        var act = () => _sut.Unsubscribe("nonexistent");

        act.Should().NotThrow();
    }

    [Fact]
    public async Task BroadcastAsync_DeliversToMatchingClinic()
    {
        var result = _sut.Subscribe("conn-1", _clinicId, "Vet");
        var reader = result.Value;

        var evt = new MessagingEvent
        {
            Type = "new-message",
            ClinicId = _clinicId,
            Category = null
        };

        await _sut.BroadcastAsync(evt);

        var received = await reader.ReadAsync();
        received.Type.Should().Be("new-message");
    }
}
