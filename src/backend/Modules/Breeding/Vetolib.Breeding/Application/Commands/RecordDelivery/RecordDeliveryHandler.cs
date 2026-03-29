using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Commands.RecordDelivery;

internal class RecordDeliveryHandler : IRequestHandler<RecordDeliveryCommand, Result<PregnancyDto>>
{
    private readonly BreedingDbContext _context;

    public RecordDeliveryHandler(BreedingDbContext context) => _context = context;

    public async Task<Result<PregnancyDto>> Handle(RecordDeliveryCommand cmd, CancellationToken ct)
    {
        var pregnancy = await _context.Pregnancies
            .Include(p => p.ScheduledChecks)
            .FirstOrDefaultAsync(p => p.Id == cmd.PregnancyId, ct);

        if (pregnancy is null)
            return Result<PregnancyDto>.NotFound($"Pregnancy '{cmd.PregnancyId}' not found.");

        var result = pregnancy.RecordDelivery(cmd.DeliveryDate, cmd.Outcome, cmd.OffspringCount, cmd.Notes);
        if (!result.IsSuccess)
            return Result<PregnancyDto>.Error(new ErrorList(result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<PregnancyDto>.Success(pregnancy.ToDto());
    }
}
