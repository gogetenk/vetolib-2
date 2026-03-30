namespace Vetolib.MedicalRecords.Contracts;

public record PatientSummaryDto(
    PatientSummaryPatientInfoDto Patient,
    PatientSummaryOwnerInfoDto? Owner,
    IReadOnlyList<PatientSummaryMedicalRecordDto> RecentMedicalRecords,
    IReadOnlyList<PatientSummaryPrescriptionDto> ActivePrescriptions,
    IReadOnlyList<PatientSummaryVaccinationDto> Vaccinations,
    IReadOnlyList<string> HealthAlerts,
    DateTime GeneratedAtUtc);

public record PatientSummaryPatientInfoDto(
    Guid Id,
    string Name,
    Species Species,
    string Breed,
    Sex Sex,
    DateOnly BirthDate,
    string? MicrochipNumber,
    decimal? LatestWeightKg);

public record PatientSummaryOwnerInfoDto(
    string FullName,
    string Email,
    string? Phone);

public record PatientSummaryMedicalRecordDto(
    Guid Id,
    string Diagnosis,
    string Treatment,
    string VetName,
    DateTime ExaminedAt);

public record PatientSummaryPrescriptionDto(
    Guid Id,
    string Medication,
    string Dosage,
    DateTime CreatedAt);

public record PatientSummaryVaccinationDto(
    string Name,
    DateTime AdministeredAt,
    string VetName);
