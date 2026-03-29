using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.RecordLoss;

internal record RecordLossCommand(
    Guid PregnancyId,
    DateOnly LossDate,
    PregnancyOutcome Outcome,
    string? Notes
) : IRequest<Result<PregnancyDto>>;
