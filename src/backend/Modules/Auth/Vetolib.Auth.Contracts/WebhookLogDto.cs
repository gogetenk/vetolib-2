namespace Vetolib.Auth.Contracts;

public record WebhookLogDto(
    Guid Id,
    Guid WebhookRegistrationId,
    string EventType,
    string Status,
    DateTime ReceivedAt,
    DateTime? ProcessedAt);
