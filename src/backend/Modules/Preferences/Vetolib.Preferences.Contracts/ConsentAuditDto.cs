namespace Vetolib.Preferences.Contracts;

public record ConsentAuditDto(
    Guid Id,
    Guid UserId,
    PreferenceCategory Category,
    PreferenceKey Key,
    string? PreviousValue,
    string NewValue,
    PreferenceSource Source,
    DateTime CreatedAt
);
