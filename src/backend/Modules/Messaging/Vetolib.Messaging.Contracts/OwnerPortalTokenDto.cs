namespace Vetolib.Messaging.Contracts;

public record OwnerPortalTokenDto(
    Guid Id,
    Guid ClinicId,
    Guid OwnerId,
    string Token,
    DateTime ExpiresAt,
    DateTime? ConsentAcceptedAt,
    string? ConsentVersion,
    DateTime CreatedAt
);
