namespace Vetolib.Breeding.Contracts;

public record SetLineageRequest(
    Guid? MotherPatientId,
    Guid? FatherPatientId,
    string? RegistryNumber,
    RegistryType? RegistryType);
