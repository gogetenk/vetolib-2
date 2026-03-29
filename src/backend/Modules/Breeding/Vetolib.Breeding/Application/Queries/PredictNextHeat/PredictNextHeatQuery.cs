using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.PredictNextHeat;

internal record PredictNextHeatQuery(Guid PatientId) : IRequest<Result<HeatPredictionDto>>;
