using System.Collections.Concurrent;
using Ardalis.Result;
using Vetolib.Billing.Contracts.EInvoicing;

namespace Vetolib.Billing.Infrastructure;

/// <summary>
/// Mock implementation of <see cref="IEInvoicingGateway"/> for development and testing.
/// Always returns Submitted on first call, then Accepted after 2 GetStatus calls.
/// </summary>
internal sealed class MockEInvoicingGateway : IEInvoicingGateway
{
    private readonly ConcurrentDictionary<string, int> _statusCallCounts = new();

    public Task<Result<EInvoiceSubmissionResult>> SubmitInvoiceAsync(EInvoicePayload payload, CancellationToken ct)
    {
        var platformId = $"MOCK-{Guid.NewGuid():N}"[..20];

        var result = new EInvoiceSubmissionResult(
            PlatformInvoiceId: platformId,
            Status: EInvoicingPlatformStatus.Submitted,
            SubmittedAt: DateTime.UtcNow);

        _statusCallCounts[platformId] = 0;

        return Task.FromResult(Result<EInvoiceSubmissionResult>.Success(result));
    }

    public Task<Result<EInvoiceStatus>> GetStatusAsync(string platformInvoiceId, CancellationToken ct)
    {
        if (!_statusCallCounts.ContainsKey(platformInvoiceId))
        {
            return Task.FromResult(
                Result<EInvoiceStatus>.NotFound($"PLATFORM_INVOICE_NOT_FOUND:No invoice found with id {platformInvoiceId}"));
        }

        var callCount = _statusCallCounts.AddOrUpdate(platformInvoiceId, 1, (_, count) => count + 1);

        var status = callCount >= 2
            ? EInvoicingPlatformStatus.Accepted
            : EInvoicingPlatformStatus.Submitted;

        var result = new EInvoiceStatus(
            PlatformInvoiceId: platformInvoiceId,
            Status: status,
            RejectionReason: null);

        return Task.FromResult(Result<EInvoiceStatus>.Success(result));
    }
}
