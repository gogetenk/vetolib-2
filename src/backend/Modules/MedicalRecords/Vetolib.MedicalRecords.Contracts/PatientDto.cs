namespace Vetolib.MedicalRecords.Contracts;

public record PatientDto(
    Guid Id,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    string OwnerName,
    string OwnerPhone,
    Guid ClinicId,
    string? MicrochipNumber = null);
