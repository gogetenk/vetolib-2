namespace Vetolib.Breeding.Contracts;

public record RecordHeatCycleRequest(
    DateOnly StartDate,
    DateOnly? EndDate = null,
    string? Notes = null);
