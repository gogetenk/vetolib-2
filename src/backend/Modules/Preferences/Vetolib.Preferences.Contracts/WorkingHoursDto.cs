namespace Vetolib.Preferences.Contracts;

public record WorkingHoursDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    bool IsOpen,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    TimeOnly? BreakStartTime,
    TimeOnly? BreakEndTime);
