using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetPedigree;

internal record GetPedigreeQuery(Guid PatientId, int Generations = 3) : IRequest<Result<PedigreeNodeDto>>;
