namespace Vetolib.Breeding.Contracts;

public record CreatePregnancyRequest(
    Guid PatientId,
    Guid? FatherPatientId,
    DateOnly MatingDate,
    MatingMethod MatingMethod,
    string? Notes);
