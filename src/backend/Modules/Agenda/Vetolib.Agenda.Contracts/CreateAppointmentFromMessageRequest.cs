namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Used by the Messaging module to create an appointment pre-filled from a conversation.
/// Cross-module communication via Contracts — never via runtime reference.
/// </summary>
public record CreateAppointmentFromMessageRequest(
    Guid ConversationId,
    Guid PatientId,
    Guid OwnerId,
    string AnimalName,
    string OwnerName,
    string? Notes,
    DateOnly? PreferredDate);
