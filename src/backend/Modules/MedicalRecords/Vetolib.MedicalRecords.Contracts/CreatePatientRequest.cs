namespace Vetolib.MedicalRecords.Contracts;

public record CreatePatientRequest(
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    string OwnerName,
    string OwnerPhone);
