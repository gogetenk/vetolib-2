namespace Vetolib.MedicalRecords.Contracts;

public record WeightHistoryResultDto(
    IReadOnlyList<WeightEntryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
