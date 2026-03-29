using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetHeatCycles;

internal class GetHeatCyclesHandler : IRequestHandler<GetHeatCyclesQuery, Result<IReadOnlyList<HeatCycleDto>>>
{
    private readonly BreedingDbContext _context;

    public GetHeatCyclesHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<HeatCycleDto>>> Handle(GetHeatCyclesQuery query, CancellationToken ct)
    {
        var cycles = await _context.HeatCycles
            .Where(h => h.PatientId == query.PatientId)
            .OrderBy(h => h.StartDate)
            .ToListAsync(ct);

        var dtos = cycles.Select(c => c.ToDto()).ToList();
        return Result<IReadOnlyList<HeatCycleDto>>.Success(dtos);
    }
}
