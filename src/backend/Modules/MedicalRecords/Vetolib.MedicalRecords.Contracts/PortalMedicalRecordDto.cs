namespace Vetolib.MedicalRecords.Contracts;

public record PortalMedicalRecordDto(
    Guid Id,
    string Diagnosis,
    string Treatment,
    string VetName,
    DateTime ExaminedAt);
