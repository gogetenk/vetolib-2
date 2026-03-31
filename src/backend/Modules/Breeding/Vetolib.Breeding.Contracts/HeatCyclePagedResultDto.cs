namespace Vetolib.Breeding.Contracts;

public record HeatCyclePagedResultDto(
    IReadOnlyList<HeatCycleDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
