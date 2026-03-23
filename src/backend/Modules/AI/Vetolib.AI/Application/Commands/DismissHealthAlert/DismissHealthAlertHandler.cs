using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Commands.DismissHealthAlert;

internal class DismissHealthAlertHandler : IRequestHandler<DismissHealthAlertCommand, Result>
{
    private readonly AIDbContext _context;

    public DismissHealthAlertHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DismissHealthAlertCommand cmd, CancellationToken ct)
    {
        var alert = await _context.HealthAlerts
            .FirstOrDefaultAsync(a => a.Id == cmd.AlertId, ct);

        if (alert is null)
            return Result.NotFound($"Health alert {cmd.AlertId} not found.");

        var result = alert.Dismiss(cmd.Reason, cmd.DismissedByName);
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
