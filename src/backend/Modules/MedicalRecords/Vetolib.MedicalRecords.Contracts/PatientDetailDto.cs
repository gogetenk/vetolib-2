namespace Vetolib.MedicalRecords.Contracts;

public record MedicalRecordSummaryDto(
    Guid Id,
    string Diagnosis,
    string VetName,
    DateTime ExaminedAt);

public record PatientDetailDto(
    PatientDto Patient,
    IReadOnlyList<MedicalRecordSummaryDto> RecentRecords);
