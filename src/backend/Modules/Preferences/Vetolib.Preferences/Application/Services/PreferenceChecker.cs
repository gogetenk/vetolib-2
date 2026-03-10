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
        Guid userId,
        PreferenceKey key,
        CancellationToken ct = default)
    {
        var userPref = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == key, ct);
        if (userPref is not null)
            return Result<string>.Success(userPref.Value);

        var clinicPref = await _context.ClinicPreferenceDefaults
            .FirstOrDefaultAsync(p => p.Key == key, ct);
        if (clinicPref is not null)
            return Result<string>.Success(clinicPref.Value);

        return Result<string>.Success(SystemDefaults.GetDefault(key));
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
}
