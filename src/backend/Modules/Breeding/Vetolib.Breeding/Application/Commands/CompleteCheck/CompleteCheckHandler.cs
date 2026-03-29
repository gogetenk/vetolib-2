using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Commands.CompleteCheck;

internal class CompleteCheckHandler : IRequestHandler<CompleteCheckCommand, Result<PregnancyCheckDto>>
{
    private readonly BreedingDbContext _context;

    public CompleteCheckHandler(BreedingDbContext context) => _context = context;

    public async Task<Result<PregnancyCheckDto>> Handle(CompleteCheckCommand cmd, CancellationToken ct)
    {
        var check = await _context.PregnancyChecks
            .FirstOrDefaultAsync(c => c.Id == cmd.CheckId, ct);

        if (check is null)
            return Result<PregnancyCheckDto>.NotFound($"PregnancyCheck '{cmd.CheckId}' not found.");

        var result = check.Complete(cmd.Result);
        if (!result.IsSuccess)
            return Result<PregnancyCheckDto>.Error(new ErrorList(result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<PregnancyCheckDto>.Success(check.ToDto());
    }
}
