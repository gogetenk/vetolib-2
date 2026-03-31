namespace Vetolib.Auth.Contracts;

public record ClinicSearchPagedResultDto(
    IReadOnlyList<ClinicSearchResultDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
