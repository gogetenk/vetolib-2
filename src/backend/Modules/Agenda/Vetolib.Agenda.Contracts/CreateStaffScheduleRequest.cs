namespace Vetolib.Agenda.Contracts;

public record CreateStaffScheduleRequest(
    Guid UserId,
    string UserName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    ShiftType ShiftType,
    bool IsAvailable = true);
