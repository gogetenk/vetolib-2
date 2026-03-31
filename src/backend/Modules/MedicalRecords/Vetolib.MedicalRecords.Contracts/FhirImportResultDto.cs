namespace Vetolib.MedicalRecords.Contracts;

public record FhirImportResultDto(
    Guid PatientId,
    string PatientName,
    bool WasMerged,
    int MedicalRecordsImported,
    int PrescriptionsImported,
    int WeightEntriesImported,
    IReadOnlyList<string> Warnings);
