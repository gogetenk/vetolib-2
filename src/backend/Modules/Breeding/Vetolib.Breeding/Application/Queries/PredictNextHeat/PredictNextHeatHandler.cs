using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.PredictNextHeat;

internal class PredictNextHeatHandler : IRequestHandler<PredictNextHeatQuery, Result<HeatPredictionDto>>
{
    private readonly BreedingDbContext _context;

    public PredictNextHeatHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<HeatPredictionDto>> Handle(PredictNextHeatQuery query, CancellationToken ct)
    {
        var cycles = await _context.HeatCycles
            .Where(h => h.PatientId == query.PatientId)
            .OrderBy(h => h.StartDate)
            .ToListAsync(ct);

        return HeatCyclePrediction.Predict(cycles);
    }
}
