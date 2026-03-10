namespace Vetolib.Preferences.Contracts;

public record PreferenceCategoryDto(
    PreferenceCategory Category,
    string DisplayName,
    string Description,
    IReadOnlyList<PreferenceDto> Preferences
);
