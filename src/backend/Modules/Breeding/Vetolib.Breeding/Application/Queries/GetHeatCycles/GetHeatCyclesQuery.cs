using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetHeatCycles;

internal record GetHeatCyclesQuery(Guid PatientId, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<HeatCyclePagedResultDto>>;
