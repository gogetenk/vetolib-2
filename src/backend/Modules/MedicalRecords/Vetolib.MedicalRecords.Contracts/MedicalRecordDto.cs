namespace Vetolib.MedicalRecords.Contracts;

public record MedicalRecordDto(
    Guid Id,
    Guid PatientId,
    Guid ClinicId,
    string Diagnosis,
    string Treatment,
    string VetName,
    DateTime ExaminedAt,
    List<PrescriptionDto> Prescriptions);
