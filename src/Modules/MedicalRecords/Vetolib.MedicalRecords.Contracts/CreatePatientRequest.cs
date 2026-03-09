namespace Vetolib.MedicalRecords.Contracts;

public record CreatePatientRequest(
    string Name,
    string Species,
    string Breed,
    DateTime? DateOfBirth,
    Guid OwnerId);
