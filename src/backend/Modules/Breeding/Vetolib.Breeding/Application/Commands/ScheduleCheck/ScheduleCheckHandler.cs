using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Commands.ScheduleCheck;

internal class ScheduleCheckHandler : IRequestHandler<ScheduleCheckCommand, Result<PregnancyCheckDto>>
{
    private readonly BreedingDbContext _context;

    public ScheduleCheckHandler(BreedingDbContext context) => _context = context;

    public async Task<Result<PregnancyCheckDto>> Handle(ScheduleCheckCommand cmd, CancellationToken ct)
    {
        var pregnancy = await _context.Pregnancies
            .Include(p => p.ScheduledChecks)
            .FirstOrDefaultAsync(p => p.Id == cmd.PregnancyId, ct);

        if (pregnancy is null)
            return Result<PregnancyCheckDto>.NotFound($"Pregnancy '{cmd.PregnancyId}' not found.");

        var checkResult = pregnancy.ScheduleCheck(cmd.ScheduledDate, cmd.CheckType, cmd.Note);
        if (!checkResult.IsSuccess)
            return checkResult.Map(_ => (PregnancyCheckDto)null!);

        await _context.SaveChangesAsync(ct);

        return Result<PregnancyCheckDto>.Success(checkResult.Value.ToDto());
    }
}
