namespace Vetolib.Agenda.Contracts;

public record StaffScheduleDto(
    Guid Id,
    Guid ClinicId,
    Guid UserId,
    string UserName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    ShiftType ShiftType,
    bool IsAvailable);
