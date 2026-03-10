namespace Vetolib.MedicalRecords.Contracts;

public record PatientSpeciesWeightDto(
    Guid PatientId,
    Species Species,
    decimal? WeightKg);
