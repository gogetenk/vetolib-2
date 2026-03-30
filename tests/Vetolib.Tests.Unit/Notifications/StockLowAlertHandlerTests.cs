using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Vetolib.Notifications.Consumers;
using Vetolib.Stock.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class StockLowAlertHandlerTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid StockItemId = new("22222222-2222-2222-2222-222222222222");

    private readonly TestLogger _logger;
    private readonly StockLowAlertHandler _handler;

    public StockLowAlertHandlerTests()
    {
        _logger = new TestLogger();
        _handler = new StockLowAlertHandler(_logger);
    }

    private static StockLowEvent BuildEvent(
        int currentQuantity = 3,
        int minThreshold = 10,
        string itemName = "Amoxicillin 250mg") =>
        new(ClinicId, StockItemId, itemName, currentQuantity, minThreshold);

    [Fact]
    public async Task Handle_WhenCalled_DoesNotThrow()
    {
        var evt = BuildEvent();

        var act = async () => await _handler.Handle(evt, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenCalled_LogsWarning()
    {
        var evt = BuildEvent(itemName: "Doxycycline 100mg");

        await _handler.Handle(evt, CancellationToken.None);

        _logger.LoggedLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_WhenCalled_LogMessageContainsItemName()
    {
        var evt = BuildEvent(itemName: "Doxycycline 100mg");

        await _handler.Handle(evt, CancellationToken.None);

        _logger.LoggedMessage.Should().Contain("Doxycycline 100mg");
    }

    [Fact]
    public async Task Handle_WhenCalled_LogMessageContainsQuantityAndThreshold()
    {
        var evt = BuildEvent(currentQuantity: 2, minThreshold: 15);

        await _handler.Handle(evt, CancellationToken.None);

        _logger.LoggedMessage.Should().Contain("2");
        _logger.LoggedMessage.Should().Contain("15");
    }

    [Fact]
    public async Task Handle_WhenCalled_LogMessageContainsClinicId()
    {
        var evt = BuildEvent();

        await _handler.Handle(evt, CancellationToken.None);

        _logger.LoggedMessage.Should().Contain(ClinicId.ToString());
    }

    /// <summary>
    /// Simple test logger that captures the last log entry for assertions.
    /// Avoids NSubstitute proxy issues with internal types in ILogger generic parameter.
    /// </summary>
    private sealed class TestLogger : ILogger<StockLowAlertHandler>
    {
        public LogLevel? LoggedLevel { get; private set; }
        public string? LoggedMessage { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            LoggedLevel = logLevel;
            LoggedMessage = formatter(state, exception);
        }
    }
}
