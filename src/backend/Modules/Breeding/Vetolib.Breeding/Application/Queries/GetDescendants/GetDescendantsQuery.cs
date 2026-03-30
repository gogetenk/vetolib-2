using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetDescendants;

internal record GetDescendantsQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<PedigreeNodeDto>>>;
