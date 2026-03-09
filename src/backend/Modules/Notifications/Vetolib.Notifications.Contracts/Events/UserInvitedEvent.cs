namespace Vetolib.Notifications.Contracts.Events;

/// <summary>
/// Published when a new user is invited to the clinic.
/// Maps to the user-invited template.
/// </summary>
public record UserInvitedEvent
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
