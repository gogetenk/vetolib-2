using Ardalis.Result;

namespace Vetolib.Preferences.Contracts;

/// <summary>
/// Cross-module interface: resolves a preference value using the cascade User > Clinic > System.
/// Consumers inject this interface from Contracts — they never reference the Preferences runtime.
/// </summary>
public interface IPreferenceChecker
{
    /// <summary>
    /// Returns the effective value for a preference key for a given user in a clinic.
    /// Resolution order: user-level override -> clinic default -> hardcoded system default.
    /// </summary>
    Task<Result<string>> GetValueAsync(
        Guid clinicId,
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default);

    /// <summary>
    /// Convenience: resolves to a boolean value for boolean preferences.
    /// </summary>
    Task<Result<bool>> IsTrueAsync(
        Guid clinicId,
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default);
}
