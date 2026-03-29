namespace Vetolib.Breeding.Contracts;

public record AddOffspringToLitterRequest(
    Guid PatientId,
    int? BirthOrder);
