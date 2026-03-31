namespace Vetolib.Agenda.Contracts;

public record WaitlistPagedResultDto(
    IReadOnlyList<WaitlistEntryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
