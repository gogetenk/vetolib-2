namespace Vetolib.Messaging.Contracts;

public record ConversationDto(
    Guid Id,
    Guid ClinicId,
    Guid OwnerId,
    Guid? PatientId,
    string Subject,
    MessageCategory Category,
    ConversationStatus Status,
    ConversationChannel Channel,
    int MessageCount,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    bool IsSpam,
    Guid? AssignedToUserId,
    string? AssignedToRole,
    decimal? AiTriageConfidence,
    bool IsTriageUncertain
);
