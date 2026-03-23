using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Commands.AcknowledgeHealthAlert;

internal class AcknowledgeHealthAlertHandler : IRequestHandler<AcknowledgeHealthAlertCommand, Result>
{
    private readonly AIDbContext _context;

    public AcknowledgeHealthAlertHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AcknowledgeHealthAlertCommand cmd, CancellationToken ct)
    {
        var alert = await _context.HealthAlerts
            .FirstOrDefaultAsync(a => a.Id == cmd.AlertId, ct);

        if (alert is null)
            return Result.NotFound($"Health alert {cmd.AlertId} not found.");

        var result = alert.Acknowledge();
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
