using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetLitterById;

internal class GetLitterByIdHandler : IRequestHandler<GetLitterByIdQuery, Result<LitterDto>>
{
    private readonly BreedingDbContext _context;

    public GetLitterByIdHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LitterDto>> Handle(GetLitterByIdQuery query, CancellationToken ct)
    {
        var litter = await _context.Litters
            .Include(l => l.Offspring)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == query.LitterId, ct);

        if (litter is null)
            return Result<LitterDto>.NotFound();

        return Result<LitterDto>.Success(litter.ToDto());
    }
}
