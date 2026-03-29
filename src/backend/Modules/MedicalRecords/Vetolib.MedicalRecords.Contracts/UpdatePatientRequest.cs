namespace Vetolib.MedicalRecords.Contracts;

public record UpdatePatientRequest(
    string? Name,
    Species? Species,
    string? Breed,
    DateOnly? BirthDate,
    string? OwnerName,
    string? OwnerPhone,
    Sex? Sex = null,
    string? MicrochipNumber = null);
