using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Commands.AddOffspringToLitter;

internal class AddOffspringToLitterHandler : IRequestHandler<AddOffspringToLitterCommand, Result<LitterDto>>
{
    private readonly BreedingDbContext _context;

    public AddOffspringToLitterHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LitterDto>> Handle(AddOffspringToLitterCommand cmd, CancellationToken ct)
    {
        var litter = await _context.Litters
            .Include(l => l.Offspring)
            .FirstOrDefaultAsync(l => l.Id == cmd.LitterId, ct);

        if (litter is null)
            return Result<LitterDto>.NotFound($"Litter '{cmd.LitterId}' not found");

        var addResult = litter.AddOffspring(cmd.PatientId, cmd.BirthOrder);
        if (!addResult.IsSuccess)
            return Result<LitterDto>.Error(string.Join("; ", addResult.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<LitterDto>.Success(litter.ToDto());
    }
}
