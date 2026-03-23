using Ardalis.Result;

namespace Vetolib.Billing.Contracts;

public interface IEReportingGateway
{
    /// <summary>
    /// Submits e-reporting data for a B2C period to the tax administration platform.
    /// Returns the platform submission ID on success.
    /// </summary>
    Task<Result<string>> SubmitEReportingAsync(
        Guid clinicId,
        DateOnly periodStart,
        DateOnly periodEnd,
        IReadOnlyList<EReportingTaxBreakdownDto> taxBreakdowns,
        int transactionCount,
        decimal totalExclTax,
        decimal totalTax,
        decimal totalInclTax,
        CancellationToken ct = default);
}
