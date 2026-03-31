namespace Vetolib.MedicalRecords.Contracts;

public record PortalWeightEntryDto(
    decimal WeightKg,
    DateTime RecordedAt);
