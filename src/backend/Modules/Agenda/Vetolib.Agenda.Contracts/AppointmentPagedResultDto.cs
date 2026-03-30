namespace Vetolib.Agenda.Contracts;

public record AppointmentPagedResultDto(
    IReadOnlyList<AppointmentDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
