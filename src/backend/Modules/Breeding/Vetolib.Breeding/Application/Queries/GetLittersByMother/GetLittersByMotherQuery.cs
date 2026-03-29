using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetLittersByMother;

internal record GetLittersByMotherQuery(Guid MotherPatientId) : IRequest<Result<IReadOnlyList<LitterDto>>>;
