using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CreateStaffSchedule;

internal record CreateStaffScheduleCommand(
    Guid ClinicId,
    Guid UserId,
    string UserName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    ShiftType ShiftType,
    bool IsAvailable) : IRequest<Result<StaffScheduleDto>>;
