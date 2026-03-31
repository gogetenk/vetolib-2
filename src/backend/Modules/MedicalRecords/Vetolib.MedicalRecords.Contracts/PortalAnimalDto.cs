namespace Vetolib.MedicalRecords.Contracts;

public record PortalAnimalDto(
    Guid Id,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    Sex Sex,
    string ClinicName,
    Guid ClinicId,
    string? MicrochipNumber);
