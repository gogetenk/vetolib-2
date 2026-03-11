using Ardalis.Result;

namespace Vetolib.Messaging.Contracts;

/// <summary>
/// Validates a magic link token and returns the associated owner/clinic identity.
/// Implemented by the Messaging module; injected into other modules (e.g. Agenda) that need portal auth.
/// </summary>
public interface IOwnerPortalTokenValidator
{
    /// <summary>
    /// Validates the token string and returns (OwnerId, ClinicId) if valid.
    /// Returns Result.Unauthorized if the token is not found or expired.
    /// </summary>
    Task<Result<OwnerPortalTokenDto>> ValidateAsync(string tokenValue, CancellationToken ct = default);
}
