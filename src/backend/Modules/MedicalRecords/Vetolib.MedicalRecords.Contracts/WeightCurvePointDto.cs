namespace Vetolib.MedicalRecords.Contracts;

public record WeightCurvePointDto(
    DateTime RecordedAt,
    decimal WeightKg);
