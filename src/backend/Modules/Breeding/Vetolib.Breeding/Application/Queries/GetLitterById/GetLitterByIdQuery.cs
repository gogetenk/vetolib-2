using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetLitterById;

internal record GetLitterByIdQuery(Guid LitterId) : IRequest<Result<LitterDto>>;
