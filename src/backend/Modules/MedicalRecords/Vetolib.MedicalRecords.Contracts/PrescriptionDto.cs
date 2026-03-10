namespace Vetolib.MedicalRecords.Contracts;

public record PrescriptionDto(
    Guid Id,
    Guid MedicalRecordId,
    Guid ClinicId,
    string Medication,
    string Dosage,
    string VetLicenseNumber,
    DateTime CreatedAt,
    Guid? DrugCatalogEntryId = null);
