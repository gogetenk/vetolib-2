using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetHeatCycles;

internal class GetHeatCyclesHandler : IRequestHandler<GetHeatCyclesQuery, Result<HeatCyclePagedResultDto>>
{
    private readonly BreedingDbContext _context;

    public GetHeatCyclesHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<HeatCyclePagedResultDto>> Handle(GetHeatCyclesQuery query, CancellationToken ct)
    {
        var page = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 200);

        var baseQuery = _context.HeatCycles
            .Where(h => h.PatientId == query.PatientId);

        var totalCount = await baseQuery.CountAsync(ct);

        var cycles = await baseQuery
            .OrderBy(h => h.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = cycles.Select(c => c.ToDto()).ToList();

        return Result<HeatCyclePagedResultDto>.Success(
            new HeatCyclePagedResultDto(dtos, totalCount, page, pageSize));
    }
}
