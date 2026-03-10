using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Queries.GetClinicDefaults;

internal class GetClinicDefaultsHandler : IRequestHandler<GetClinicDefaultsQuery, Result<List<PreferenceCategoryDto>>>
{
    private readonly PreferencesDbContext _context;

    public GetClinicDefaultsHandler(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PreferenceCategoryDto>>> Handle(GetClinicDefaultsQuery query, CancellationToken ct)
    {
        var clinicDefaults = await _context.ClinicPreferenceDefaults.ToListAsync(ct);

        var result = new List<PreferenceDto>();

        foreach (var key in Enum.GetValues<PreferenceKey>())
        {
            var clinicPref = clinicDefaults.FirstOrDefault(p => p.Key == key);
            if (clinicPref is not null)
            {
                result.Add(new PreferenceDto(clinicPref.Category, key, clinicPref.Value, PreferenceSource.Clinic));
            }
            else
            {
                var systemValue = SystemDefaults.GetDefault(key);
                var category = SystemDefaults.GetCategory(key);
                result.Add(new PreferenceDto(category, key, systemValue, PreferenceSource.System));
            }
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
