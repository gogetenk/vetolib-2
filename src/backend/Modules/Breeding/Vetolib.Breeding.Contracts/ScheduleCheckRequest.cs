namespace Vetolib.Breeding.Contracts;

public record ScheduleCheckRequest(
    DateOnly ScheduledDate,
    PregnancyCheckType CheckType,
    string? Note);
