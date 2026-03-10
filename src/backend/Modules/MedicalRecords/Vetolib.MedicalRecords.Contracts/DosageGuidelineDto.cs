namespace Vetolib.MedicalRecords.Contracts;

public record DosageGuidelineDto(
    Species Species,
    decimal MinDosePerKg,
    decimal MaxDosePerKg,
    string Unit,
    string Route);
