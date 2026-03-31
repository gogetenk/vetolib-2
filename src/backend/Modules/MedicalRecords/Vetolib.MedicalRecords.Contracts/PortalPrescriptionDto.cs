namespace Vetolib.MedicalRecords.Contracts;

public record PortalPrescriptionDto(
    Guid Id,
    string Medication,
    string Dosage,
    DateTime PrescribedAt,
    string VetName);
