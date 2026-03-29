using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetPregnancyById;

internal record GetPregnancyByIdQuery(Guid PregnancyId) : IRequest<Result<PregnancyDto>>;
