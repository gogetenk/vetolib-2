namespace Vetolib.MedicalRecords.Contracts;

public record PatientDto(
    Guid Id,
    string Name,
    string Species,
    string Breed,
    DateTime? DateOfBirth,
    Guid ClinicId,
    DateTime CreatedAt,
    List<OwnerDto> Owners);
