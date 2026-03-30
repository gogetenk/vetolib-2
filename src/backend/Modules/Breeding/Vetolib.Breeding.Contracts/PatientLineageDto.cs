namespace Vetolib.Breeding.Contracts;

public record PatientLineageDto(
    Guid PatientId,
    string PatientName,
    Guid? MotherPatientId,
    string? MotherName,
    Guid? FatherPatientId,
    string? FatherName,
    string? RegistryNumber,
    RegistryType? RegistryType);
