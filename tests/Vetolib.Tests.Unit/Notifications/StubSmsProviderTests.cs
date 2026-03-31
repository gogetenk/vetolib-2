using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Vetolib.Notifications.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class StubSmsProviderTests
{
    [Fact]
    public async Task SendSmsAsync_ReturnsSuccess()
    {
        var provider = new StubSmsProvider(NullLogger<StubSmsProvider>.Instance);

        var result = await provider.SendSmsAsync("+971501234567", "Test message");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendSmsAsync_WithCancellationToken_ReturnsSuccess()
    {
        var provider = new StubSmsProvider(NullLogger<StubSmsProvider>.Instance);

        var result = await provider.SendSmsAsync("+971501234567", "Test message", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
