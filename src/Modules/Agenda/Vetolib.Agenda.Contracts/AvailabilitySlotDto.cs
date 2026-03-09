namespace Vetolib.Agenda.Contracts;

public record AvailabilitySlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable);
