namespace Vetolib.Preferences.Contracts;

/// <summary>
/// Source indicates where the value comes from in the cascade: User > Clinic > System.
/// </summary>
public enum PreferenceSource
{
    User,
    Clinic,
    System
}

public record PreferenceDto(
    PreferenceCategory Category,
    PreferenceKey Key,
    string Value,
    PreferenceSource Source
);
