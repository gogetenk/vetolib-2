using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetActivePregnancies;

internal class GetActivePregnanciesHandler : IRequestHandler<GetActivePregnanciesQuery, Result<IReadOnlyList<PregnancyDto>>>
{
    private readonly BreedingDbContext _context;

    public GetActivePregnanciesHandler(BreedingDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<PregnancyDto>>> Handle(GetActivePregnanciesQuery query, CancellationToken ct)
    {
        var pregnancies = await _context.Pregnancies
            .AsNoTracking()
            .Include(p => p.ScheduledChecks)
            .Where(p => p.Status == PregnancyStatus.Active)
            .OrderBy(p => p.ExpectedDueDate)
            .ToListAsync(ct);

        return Result<IReadOnlyList<PregnancyDto>>.Success(
            pregnancies.Select(p => p.ToDto()).ToList());
    }
}
