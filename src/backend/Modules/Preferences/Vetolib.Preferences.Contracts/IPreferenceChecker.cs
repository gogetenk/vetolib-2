using Ardalis.Result;

namespace Vetolib.Preferences.Contracts;

public interface IPreferenceChecker
{
    Task<Result<string>> GetValueAsync(
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default);

    Task<Result<bool>> IsTrueAsync(
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default);
}
