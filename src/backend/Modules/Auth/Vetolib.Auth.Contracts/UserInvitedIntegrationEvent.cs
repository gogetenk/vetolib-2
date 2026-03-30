namespace Vetolib.Auth.Contracts;

/// <summary>
/// Integration event published to RabbitMQ when a user is invited.
/// Consumed by the Notifications module to send the invitation email.
/// </summary>
public record UserInvitedIntegrationEvent
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
