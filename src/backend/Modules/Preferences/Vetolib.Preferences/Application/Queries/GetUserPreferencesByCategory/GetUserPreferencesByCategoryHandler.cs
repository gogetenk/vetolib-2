using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Queries.GetUserPreferencesByCategory;

internal class GetUserPreferencesByCategoryHandler
    : IRequestHandler<GetUserPreferencesByCategoryQuery, Result<List<PreferenceDto>>>
{
    private readonly PreferencesDbContext _context;

    public GetUserPreferencesByCategoryHandler(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PreferenceDto>>> Handle(
        GetUserPreferencesByCategoryQuery query,
        CancellationToken ct)
    {
        var userPreferences = await _context.UserPreferences
            .Where(p => p.UserId == query.UserId && p.Category == query.Category)
            .ToListAsync(ct);

        var clinicDefaults = await _context.ClinicPreferenceDefaults
            .Where(p => p.Category == query.Category)
            .ToListAsync(ct);

        var keysInCategory = Enum.GetValues<PreferenceKey>()
            .Where(k => SystemDefaults.GetCategory(k) == query.Category);

        var result = new List<PreferenceDto>();

        foreach (var key in keysInCategory)
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
            result.Add(new PreferenceDto(query.Category, key, systemValue, PreferenceSource.System));
        }

        return Result<List<PreferenceDto>>.Success(result);
    }
}
