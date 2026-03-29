using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetPregnancyById;

internal class GetPregnancyByIdHandler : IRequestHandler<GetPregnancyByIdQuery, Result<PregnancyDto>>
{
    private readonly BreedingDbContext _context;

    public GetPregnancyByIdHandler(BreedingDbContext context) => _context = context;

    public async Task<Result<PregnancyDto>> Handle(GetPregnancyByIdQuery query, CancellationToken ct)
    {
        var pregnancy = await _context.Pregnancies
            .AsNoTracking()
            .Include(p => p.ScheduledChecks)
            .FirstOrDefaultAsync(p => p.Id == query.PregnancyId, ct);

        if (pregnancy is null)
            return Result<PregnancyDto>.NotFound();

        return Result<PregnancyDto>.Success(pregnancy.ToDto());
    }
}
