using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.DeactivateFollowUpRule;

internal class DeactivateFollowUpRuleHandler : IRequestHandler<DeactivateFollowUpRuleCommand, Result>
{
    private readonly AgendaDbContext _context;

    public DeactivateFollowUpRuleHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateFollowUpRuleCommand cmd, CancellationToken ct)
    {
        var rule = await _context.FollowUpRules
            .FirstOrDefaultAsync(r => r.Id == cmd.Id, ct);

        if (rule is null)
            return Result.NotFound($"FollowUpRule {cmd.Id} not found");

        var deactivateResult = rule.Deactivate();

        if (!deactivateResult.IsSuccess)
            return deactivateResult;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
