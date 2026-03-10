using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Application.Services;

internal class PreferenceChecker : IPreferenceChecker
{
    private readonly PreferencesDbContext _context;
    private readonly IClinicContext _clinicContext;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public PreferenceChecker(
        PreferencesDbContext context,
        IClinicContext clinicContext,
        IMemoryCache cache)
    {
        _context = context;
        _clinicContext = clinicContext;
        _cache = cache;
    }

    public async Task<Result<string>> GetValueAsync(
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default)
    {
        var clinicId = _clinicContext.ClinicId;
        var cacheKey = BuildCacheKey(clinicId, userId, key);

        if (_cache.TryGetValue(cacheKey, out string? cachedValue) && cachedValue is not null)
            return Result<string>.Success(cachedValue);

        var userPref = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == key, ct);

        string value;
        if (userPref is not null)
        {
            value = userPref.Value;
        }
        else
        {
            var clinicPref = await _context.ClinicPreferenceDefaults
                .FirstOrDefaultAsync(p => p.Key == key, ct);
            value = clinicPref is not null
                ? clinicPref.Value
                : SystemDefaults.GetDefault(key);
        }

        _cache.Set(cacheKey, value, CacheTtl);
        return Result<string>.Success(value);
    }

    public async Task<Result<bool>> IsTrueAsync(
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default)
    {
        var result = await GetValueAsync(userId, key, ct);
        if (!result.IsSuccess)
            return Result<bool>.Error(string.Join(", ", result.Errors));

        if (bool.TryParse(result.Value, out var boolValue))
            return Result<bool>.Success(boolValue);

        return Result<bool>.Error($"Preference '{key}' value '{result.Value}' is not a valid boolean.");
    }

    public void Invalidate(Guid clinicId, Guid userId, PreferenceKey key)
    {
        var cacheKey = BuildCacheKey(clinicId, userId, key);
        _cache.Remove(cacheKey);
    }

    internal static string BuildCacheKey(Guid clinicId, Guid userId, PreferenceKey key)
        => $"pref:{clinicId}:{userId}:{key}";
}
