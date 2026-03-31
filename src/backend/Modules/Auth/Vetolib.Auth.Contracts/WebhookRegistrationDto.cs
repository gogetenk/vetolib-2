namespace Vetolib.Auth.Contracts;

public record WebhookRegistrationDto(
    Guid Id,
    string Name,
    List<string> EventTypes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastCalledAt);
