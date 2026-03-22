using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.Messaging.Application.Consumers;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Contracts.Events;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class UrgentMessageClassifiedEventConsumerTests
{
    private readonly ILogger<UrgentMessageClassifiedEventConsumer> _logger;
    private readonly UrgentMessageClassifiedEventConsumer _sut;

    public UrgentMessageClassifiedEventConsumerTests()
    {
        _logger = Substitute.For<ILogger<UrgentMessageClassifiedEventConsumer>>();
        _sut = new UrgentMessageClassifiedEventConsumer(_logger);
    }

    [Fact]
    public async Task Handle_LogsUrgentMessageAlert()
    {
        var evt = new UrgentMessageClassifiedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ClassifiedUrgency.Critical,
            "My cat is not breathing");

        // Act — should not throw
        var act = async () => await _sut.Handle(evt, CancellationToken.None);

        await act.Should().NotThrowAsync();

        // Verify logging was called at Warning level
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_CompletesSuccessfully_ForAllUrgencyLevels()
    {
        foreach (var urgency in Enum.GetValues<ClassifiedUrgency>())
        {
            var evt = new UrgentMessageClassifiedEvent(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                urgency, "Test preview");

            var act = async () => await _sut.Handle(evt, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }
    }
}
