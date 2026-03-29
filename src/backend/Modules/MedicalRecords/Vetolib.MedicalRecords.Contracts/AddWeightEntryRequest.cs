namespace Vetolib.MedicalRecords.Contracts;

public record AddWeightEntryRequest(
    decimal WeightKg,
    string? Note = null);
