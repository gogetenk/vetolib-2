using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Services;

internal class PreferenceChecker : IPreferenceChecker
{
    private readonly PreferencesDbContext _context;

    public PreferenceChecker(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> GetValueAsync(
        Guid clinicId,
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default)
    {
        // 1. User-level override (highest priority)
        var userPref = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == key, ct);

        if (userPref is not null)
            return Result<string>.Success(userPref.Value);

        // 2. Clinic-level default
        // Note: multi-tenant filter is active, so ClinicId is already filtered to current clinic.
        // However, this method receives explicit clinicId for cross-clinic scenarios (background jobs).
        // We use IgnoreQueryFilters only conceptually — here we rely on the injected context
        // which is scoped to the current clinic via IClinicContext.
        var clinicPref = await _context.ClinicPreferenceDefaults
            .FirstOrDefaultAsync(p => p.Key == key, ct);

        if (clinicPref is not null)
            return Result<string>.Success(clinicPref.Value);

        // 3. Hardcoded system default
        var systemDefault = SystemDefaults.GetDefault(key);
        return Result<string>.Success(systemDefault);
    }

    public async Task<Result<bool>> IsTrueAsync(
        Guid clinicId,
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default)
    {
        var result = await GetValueAsync(clinicId, userId, key, ct);
        if (!result.IsSuccess)
            return Result<bool>.Error(string.Join(", ", result.Errors));

        if (bool.TryParse(result.Value, out var boolValue))
            return Result<bool>.Success(boolValue);

        return Result<bool>.Error($"Preference '{key}' value '{result.Value}' is not a valid boolean.");
    }
}
