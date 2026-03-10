namespace Vetolib.Messaging.Contracts.Events;

/// <summary>
/// Published when a staff member attaches a message to a patient's medical record.
/// Consumed by the MedicalRecords module to create a record note.
/// </summary>
public record MessageAttachedToRecordEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid PatientId,
    Guid ClinicId,
    string MessageBody,
    IReadOnlyList<string> AttachmentStoragePaths,
    DateTime AttachedAt);
