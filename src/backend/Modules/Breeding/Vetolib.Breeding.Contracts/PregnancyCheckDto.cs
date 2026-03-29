namespace Vetolib.Breeding.Contracts;

public record PregnancyCheckDto(
    Guid Id,
    Guid PregnancyId,
    DateOnly ScheduledDate,
    PregnancyCheckType CheckType,
    string? Note,
    DateTime? CompletedAt,
    string? Result);
