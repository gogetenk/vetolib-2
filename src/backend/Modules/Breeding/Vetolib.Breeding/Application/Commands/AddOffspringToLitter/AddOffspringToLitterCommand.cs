using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.AddOffspringToLitter;

internal record AddOffspringToLitterCommand(
    Guid LitterId,
    Guid PatientId,
    int? BirthOrder) : IRequest<Result<LitterDto>>;
