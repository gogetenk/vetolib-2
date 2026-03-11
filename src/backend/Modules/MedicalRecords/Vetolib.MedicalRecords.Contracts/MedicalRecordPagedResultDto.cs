namespace Vetolib.MedicalRecords.Contracts;

public record MedicalRecordPagedResultDto(
    IReadOnlyList<MedicalRecordDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
