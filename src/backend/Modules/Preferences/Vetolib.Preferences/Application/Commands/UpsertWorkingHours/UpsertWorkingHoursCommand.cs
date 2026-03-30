using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.UpsertWorkingHours;

internal record UpsertWorkingHoursCommand(
    Guid ClinicId,
    IReadOnlyList<WorkingHoursItemRequest> Days
) : IRequest<Result<IReadOnlyList<WorkingHoursDto>>>;

internal record WorkingHoursItemRequest(
    DayOfWeek DayOfWeek,
    bool IsOpen,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    TimeOnly? BreakStartTime,
    TimeOnly? BreakEndTime);
