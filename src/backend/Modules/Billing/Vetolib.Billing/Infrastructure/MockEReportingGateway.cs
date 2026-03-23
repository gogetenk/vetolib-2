using Ardalis.Result;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Infrastructure;

internal class MockEReportingGateway : IEReportingGateway
{
    public Task<Result<string>> SubmitEReportingAsync(
        Guid clinicId,
        DateOnly periodStart,
        DateOnly periodEnd,
        IReadOnlyList<EReportingTaxBreakdownDto> taxBreakdowns,
        int transactionCount,
        decimal totalExclTax,
        decimal totalTax,
        decimal totalInclTax,
        CancellationToken ct = default)
    {
        // Simulate a successful submission with a fake platform submission ID
        var submissionId = $"MOCK-EREPORT-{periodStart:yyyyMMdd}-{periodEnd:yyyyMMdd}-{Guid.NewGuid():N}";
        return Task.FromResult(Result<string>.Success(submissionId));
    }
}
