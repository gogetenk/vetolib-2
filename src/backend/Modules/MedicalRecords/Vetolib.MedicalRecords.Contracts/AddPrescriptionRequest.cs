namespace Vetolib.MedicalRecords.Contracts;

public record AddPrescriptionRequest(
    string Medication,
    string Dosage,
    Guid? DrugCatalogEntryId = null,
    decimal? DosageAmount = null,
    string? OverrideJustification = null);
