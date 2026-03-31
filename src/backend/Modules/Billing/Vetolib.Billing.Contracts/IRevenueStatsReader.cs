using Ardalis.Result;

namespace Vetolib.Billing.Contracts;

/// <summary>
/// Provides cross-clinic revenue statistics.
/// Used by the clinic group dashboard to aggregate revenue across multiple clinics.
/// </summary>
public interface IRevenueStatsReader
{
    /// <summary>
    /// Returns the total revenue (PAID invoices) for each of the specified clinic IDs.
    /// Uses IgnoreQueryFilters to bypass tenant filtering — caller must verify ownership.
    /// </summary>
    Task<Result<IReadOnlyDictionary<Guid, decimal>>> GetRevenueByClinicIdsAsync(
        IReadOnlyList<Guid> clinicIds, CancellationToken ct = default);
}
