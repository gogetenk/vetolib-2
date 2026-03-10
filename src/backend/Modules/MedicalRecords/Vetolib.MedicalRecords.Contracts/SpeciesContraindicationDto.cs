namespace Vetolib.MedicalRecords.Contracts;

public record SpeciesContraindicationDto(
    Species Species,
    InteractionSeverity Severity,
    string Reason);
