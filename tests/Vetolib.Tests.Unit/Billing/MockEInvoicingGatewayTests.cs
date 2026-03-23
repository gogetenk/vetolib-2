using FluentAssertions;
using Vetolib.Billing.Contracts.EInvoicing;
using Vetolib.Billing.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class MockEInvoicingGatewayTests
{
    private readonly MockEInvoicingGateway _gateway = new();

    [Fact]
    public async Task SubmitInvoiceAsync_ReturnsSubmittedStatus()
    {
        var payload = new EInvoicePayload(
            "INV-001", [0x01], [0x02],
            "123456789", "FR12345678901",
            "Jean Dupont", null);

        var result = await _gateway.SubmitInvoiceAsync(payload, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EInvoicingPlatformStatus.Submitted);
        result.Value.PlatformInvoiceId.Should().NotBeNullOrEmpty();
        result.Value.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetStatusAsync_FirstCall_ReturnsSubmitted()
    {
        var payload = new EInvoicePayload(
            "INV-001", [0x01], [0x02],
            "123456789", "FR12345678901",
            "Jean Dupont", null);

        var submitResult = await _gateway.SubmitInvoiceAsync(payload, CancellationToken.None);
        var platformId = submitResult.Value.PlatformInvoiceId;

        var result = await _gateway.GetStatusAsync(platformId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EInvoicingPlatformStatus.Submitted);
    }

    [Fact]
    public async Task GetStatusAsync_AfterTwoCalls_ReturnsAccepted()
    {
        var payload = new EInvoicePayload(
            "INV-001", [0x01], [0x02],
            "123456789", "FR12345678901",
            "Jean Dupont", null);

        var submitResult = await _gateway.SubmitInvoiceAsync(payload, CancellationToken.None);
        var platformId = submitResult.Value.PlatformInvoiceId;

        // First call — still Submitted
        await _gateway.GetStatusAsync(platformId, CancellationToken.None);

        // Second call — should be Accepted
        var result = await _gateway.GetStatusAsync(platformId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EInvoicingPlatformStatus.Accepted);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownId_ReturnsNotFound()
    {
        var result = await _gateway.GetStatusAsync("UNKNOWN-ID", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }
}
