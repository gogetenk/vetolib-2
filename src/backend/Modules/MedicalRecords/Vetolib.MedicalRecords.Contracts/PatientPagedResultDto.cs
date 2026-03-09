namespace Vetolib.MedicalRecords.Contracts;

public record PatientPagedResultDto(
    IReadOnlyList<PatientDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
