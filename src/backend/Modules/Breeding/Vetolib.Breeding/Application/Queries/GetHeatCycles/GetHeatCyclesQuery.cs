using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetHeatCycles;

internal record GetHeatCyclesQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<HeatCycleDto>>>;
