using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Messaging.Contracts;

public record ConversationWithMessagesDto(
    Guid Id,
    Guid ClinicId,
    Guid OwnerId,
    Guid? PatientId,
    string Subject,
    MessageCategory Category,
    ConversationStatus Status,
    ConversationChannel Channel,
    Guid? AssignedToUserId,
    string? AssignedToRole,
    decimal? AiTriageConfidence,
    bool IsTriageUncertain,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    IReadOnlyList<MessageDto> Messages,
    IReadOnlyList<string> AiSuggestedReplies,
    /// <summary>
    /// Patient context populated when a staff member opens the conversation.
    /// Null if the conversation is not linked to a patient or patient data is unavailable.
    /// The depth of the data depends on the caller's role (receptionist vs vet/admin).
    /// </summary>
    PatientContextDto? PatientContext = null
);
