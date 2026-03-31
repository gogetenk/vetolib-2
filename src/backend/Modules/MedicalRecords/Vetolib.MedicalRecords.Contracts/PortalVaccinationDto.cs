namespace Vetolib.MedicalRecords.Contracts;

public record PortalVaccinationDto(
    Guid Id,
    string Medication,
    string Dosage,
    DateTime AdministeredAt,
    string VetName);
