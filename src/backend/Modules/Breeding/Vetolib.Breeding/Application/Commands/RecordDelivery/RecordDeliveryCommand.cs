using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.RecordDelivery;

internal record RecordDeliveryCommand(
    Guid PregnancyId,
    DateOnly DeliveryDate,
    PregnancyOutcome Outcome,
    int OffspringCount,
    string? Notes
) : IRequest<Result<PregnancyDto>>;
