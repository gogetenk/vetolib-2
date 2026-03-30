using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetLineage;

internal record GetLineageQuery(Guid PatientId) : IRequest<Result<PatientLineageDto>>;
