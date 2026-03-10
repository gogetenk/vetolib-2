namespace Vetolib.Preferences.Contracts;

public record ClinicDefaultItemRequest(PreferenceKey Key, string Value);

public record UpdateClinicDefaultsRequest(IReadOnlyList<ClinicDefaultItemRequest> Defaults);
