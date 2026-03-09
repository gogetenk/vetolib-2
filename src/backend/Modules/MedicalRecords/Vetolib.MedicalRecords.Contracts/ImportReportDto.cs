namespace Vetolib.MedicalRecords.Contracts;

public record ImportReportDto(
    int Imported,
    int Skipped,
    IReadOnlyList<string> Errors);
