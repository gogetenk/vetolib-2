namespace Vetolib.Breeding.Domain;

/// <summary>
/// Species-specific gestation periods in days.
/// </summary>
internal static class GestationPeriods
{
    public static int GetDays(string species)
    {
        return species.ToUpperInvariant() switch
        {
            "DOG" => 63,
            "CAT" => 65,
            "HORSE" => 340,
            "CAMEL" => 390,
            "FALCON" => 32,
            "RABBIT" => 31,
            _ => 60
        };
    }
}
