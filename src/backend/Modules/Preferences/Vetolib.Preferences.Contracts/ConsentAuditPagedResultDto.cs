namespace Vetolib.Preferences.Contracts;

public record ConsentAuditPagedResultDto(
    IReadOnlyList<ConsentAuditDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
