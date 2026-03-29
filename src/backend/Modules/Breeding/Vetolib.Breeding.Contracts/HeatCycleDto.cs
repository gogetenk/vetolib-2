namespace Vetolib.Breeding.Contracts;

public record HeatCycleDto(
    Guid Id,
    Guid PatientId,
    DateOnly StartDate,
    DateOnly? EndDate,
    int? DurationDays,
    string? Notes);
