namespace Vetolib.Breeding.Contracts;

public record PregnancyDto(
    Guid Id,
    Guid PatientId,
    Guid? FatherPatientId,
    DateOnly MatingDate,
    MatingMethod MatingMethod,
    DateOnly ExpectedDueDate,
    DateOnly? ActualDeliveryDate,
    PregnancyOutcome? Outcome,
    int? OffspringCount,
    PregnancyStatus Status,
    string? Notes,
    IReadOnlyList<PregnancyCheckDto> ScheduledChecks,
    DateTime CreatedAt,
    DateTime UpdatedAt);
