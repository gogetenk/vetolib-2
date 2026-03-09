namespace Vetolib.MedicalRecords.Contracts;

public record AddPrescriptionRequest(
    string Medication,
    string Dosage);
