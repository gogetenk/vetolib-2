namespace Vetolib.Messaging.Contracts;

public record ConversationDto(
    Guid Id,
    Guid ClinicId,
    Guid OwnerId,
    Guid? PatientId,
    string Subject,
    MessageCategory Category,
    ConversationStatus Status,
    int MessageCount,
    DateTime CreatedAt,
    DateTime? LastMessageAt
);
