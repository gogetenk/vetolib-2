namespace Vetolib.MedicalRecords.Contracts;

public record TransferPatientResultDto(
    Guid SourcePatientId,
    Guid TargetPatientId,
    Guid SourceClinicId,
    Guid TargetClinicId,
    DateTime TransferredAt,
    int MedicalRecordsTransferred,
    int WeightEntriesTransferred,
    IReadOnlyList<string> Warnings);
