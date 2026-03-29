using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetLittersByMother;

internal class GetLittersByMotherHandler : IRequestHandler<GetLittersByMotherQuery, Result<IReadOnlyList<LitterDto>>>
{
    private readonly BreedingDbContext _context;

    public GetLittersByMotherHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<LitterDto>>> Handle(GetLittersByMotherQuery query, CancellationToken ct)
    {
        var litters = await _context.Litters
            .Include(l => l.Offspring)
            .AsNoTracking()
            .Where(l => l.MotherPatientId == query.MotherPatientId)
            .OrderByDescending(l => l.BirthDate)
            .ToListAsync(ct);

        var dtos = litters.Select(l => l.ToDto()).ToList();
        return Result<IReadOnlyList<LitterDto>>.Success(dtos);
    }
}
