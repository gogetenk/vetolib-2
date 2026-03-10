using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

/// <summary>
/// Implements IPatientRecordWriter — called by the Messaging module to attach a message as a medical record note.
/// No runtime cross-reference: Messaging references only MedicalRecords.Contracts.
/// </summary>
internal class PatientRecordWriter : IPatientRecordWriter
{
    private readonly MedicalRecordsDbContext _context;

    public PatientRecordWriter(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MedicalRecordNoteDto>> AttachMessageNoteAsync(
        AddMessageToRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId, cancellationToken);

        if (!patientExists)
            return Result<MedicalRecordNoteDto>.NotFound($"Patient {request.PatientId} not found");

        // Build the note body: message text + attachment URLs
        var noteBody = new System.Text.StringBuilder();
        noteBody.AppendLine("[From Messaging]");
        noteBody.AppendLine(request.MessageBody);

        if (request.AttachmentUrls.Count > 0)
        {
            noteBody.AppendLine();
            noteBody.AppendLine("Attachments:");
            foreach (var url in request.AttachmentUrls)
                noteBody.AppendLine($"- {url}");
        }

        var clinicId = (await _context.Patients
            .Where(p => p.Id == request.PatientId)
            .Select(p => p.ClinicId)
            .FirstAsync(cancellationToken));

        var recordResult = MedicalRecord.Create(
            clinicId,
            request.PatientId,
            diagnosis: $"Messaging note (conversation {request.ConversationId})",
            treatment: noteBody.ToString().Trim(),
            vetName: "Staff (via Messaging)",
            examinedAt: DateTime.UtcNow);

        if (!recordResult.IsSuccess)
            return Result<MedicalRecordNoteDto>.Invalid(recordResult.ValidationErrors.ToList());

        _context.MedicalRecords.Add(recordResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<MedicalRecordNoteDto>.Success(new MedicalRecordNoteDto(
            recordResult.Value.Id,
            request.PatientId,
            noteBody.ToString().Trim(),
            recordResult.Value.ExaminedAt));
    }
}
