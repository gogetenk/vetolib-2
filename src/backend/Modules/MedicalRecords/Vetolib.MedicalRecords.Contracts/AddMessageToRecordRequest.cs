namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Used by the Messaging module to attach a message to a medical record.
/// Cross-module communication via Contracts — never via runtime reference.
/// </summary>
public record AddMessageToRecordRequest(
    Guid ConversationId,
    Guid MessageId,
    Guid PatientId,
    string MessageBody,
    IReadOnlyList<string> AttachmentUrls);
