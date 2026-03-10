namespace Vetolib.Preferences.Contracts;

public record PreferenceUpdateItemRequest(PreferenceKey Key, string Value);

public record BulkUpdatePreferencesRequest(IReadOnlyList<PreferenceUpdateItemRequest> Preferences);
