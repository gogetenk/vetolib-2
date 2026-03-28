namespace Vetolib.MedicalRecords.Contracts;

public record PatientDto(
    Guid Id,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    Sex Sex,
    string OwnerName,
    string OwnerPhone,
    Guid ClinicId);
