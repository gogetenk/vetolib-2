using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Queries.GetUserPreferences;

internal class GetUserPreferencesHandler : IRequestHandler<GetUserPreferencesQuery, Result<List<PreferenceCategoryDto>>>
{
    private readonly PreferencesDbContext _context;

    public GetUserPreferencesHandler(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PreferenceCategoryDto>>> Handle(GetUserPreferencesQuery query, CancellationToken ct)
    {
        var userPreferences = await _context.UserPreferences
            .Where(p => p.UserId == query.UserId)
            .ToListAsync(ct);

        var clinicDefaults = await _context.ClinicPreferenceDefaults
            .ToListAsync(ct);

        var result = new List<PreferenceDto>();

        foreach (var key in Enum.GetValues<PreferenceKey>())
        {
            var userPref = userPreferences.FirstOrDefault(p => p.Key == key);
            if (userPref is not null)
            {
                result.Add(new PreferenceDto(userPref.Category, key, userPref.Value, PreferenceSource.User));
                continue;
            }

            var clinicPref = clinicDefaults.FirstOrDefault(p => p.Key == key);
            if (clinicPref is not null)
            {
                result.Add(new PreferenceDto(clinicPref.Category, key, clinicPref.Value, PreferenceSource.Clinic));
                continue;
            }

            var systemValue = SystemDefaults.GetDefault(key);
            var category = SystemDefaults.GetCategory(key);
            result.Add(new PreferenceDto(category, key, systemValue, PreferenceSource.System));
        }

        var grouped = result
            .GroupBy(p => p.Category)
            .Select(g => new PreferenceCategoryDto(
                g.Key,
                g.Key.ToString(),
                $"{g.Key} preferences",
                g.ToList().AsReadOnly()))
            .ToList();

        return Result<List<PreferenceCategoryDto>>.Success(grouped);
    }
}
