using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.ScheduleCheck;

internal record ScheduleCheckCommand(
    Guid PregnancyId,
    DateOnly ScheduledDate,
    PregnancyCheckType CheckType,
    string? Note
) : IRequest<Result<PregnancyCheckDto>>;
