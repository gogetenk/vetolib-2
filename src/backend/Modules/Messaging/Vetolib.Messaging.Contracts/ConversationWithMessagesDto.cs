namespace Vetolib.Messaging.Contracts;

public record ConversationWithMessagesDto(
    Guid Id,
    Guid ClinicId,
    Guid OwnerId,
    Guid? PatientId,
    string Subject,
    MessageCategory Category,
    ConversationStatus Status,
    Guid? AssignedToUserId,
    string? AssignedToRole,
    decimal? AiTriageConfidence,
    bool IsTriageUncertain,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    IReadOnlyList<MessageDto> Messages
);
