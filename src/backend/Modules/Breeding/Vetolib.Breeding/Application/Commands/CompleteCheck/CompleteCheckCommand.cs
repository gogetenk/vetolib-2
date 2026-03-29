using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.CompleteCheck;

internal record CompleteCheckCommand(
    Guid CheckId,
    string? Result
) : IRequest<Result<PregnancyCheckDto>>;
