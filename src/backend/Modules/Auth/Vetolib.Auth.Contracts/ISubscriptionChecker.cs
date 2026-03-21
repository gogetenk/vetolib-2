using Ardalis.Result;

namespace Vetolib.Auth.Contracts;

/// <summary>
/// Checks whether a clinic's current usage is within its subscription plan limits.
/// </summary>
public interface ISubscriptionChecker
{
    /// <summary>
    /// Checks if the clinic can perform an action of the given limit type.
    /// Returns Result.Success if allowed, Result.Error with upgrade message if limit exceeded.
    /// </summary>
    Task<Result> CheckLimitAsync(Guid clinicId, LimitType limitType, CancellationToken ct = default);

    /// <summary>
    /// Returns the current usage metrics for a clinic.
    /// </summary>
    Task<Result<UsageDto>> GetCurrentUsageAsync(Guid clinicId, CancellationToken ct = default);
}
