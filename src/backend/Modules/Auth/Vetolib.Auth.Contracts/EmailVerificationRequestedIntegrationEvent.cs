namespace Vetolib.Auth.Contracts;

/// <summary>
/// Integration event published to RabbitMQ when a user registers and needs email verification.
/// Consumed by the Notifications module to send the verification email.
/// </summary>
public record EmailVerificationRequestedIntegrationEvent
{
    public string Email { get; init; } = string.Empty;
    public string VerificationToken { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
