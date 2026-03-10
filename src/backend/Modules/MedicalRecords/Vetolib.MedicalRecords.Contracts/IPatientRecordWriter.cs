using Ardalis.Result;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Allows other modules to write to patient medical records without referencing the MedicalRecords runtime assembly.
/// Implement in Vetolib.MedicalRecords, register in ModuleServiceRegistrar.
/// </summary>
public interface IPatientRecordWriter
{
    /// <summary>
    /// Attaches a messaging note (message body + attachment URLs) to the patient's medical record.
    /// Creates a new MedicalRecord entry with the message content as the diagnosis note.
    /// </summary>
    Task<Result<MedicalRecordNoteDto>> AttachMessageNoteAsync(
        AddMessageToRecordRequest request,
        CancellationToken cancellationToken = default);
}

public record MedicalRecordNoteDto(Guid RecordId, Guid PatientId, string NoteBody, DateTime CreatedAt);
