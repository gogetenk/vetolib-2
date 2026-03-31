using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.UpdateFollowUpRule;

internal class UpdateFollowUpRuleHandler : IRequestHandler<UpdateFollowUpRuleCommand, Result<FollowUpRuleDto>>
{
    private readonly AgendaDbContext _context;

    public UpdateFollowUpRuleHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FollowUpRuleDto>> Handle(UpdateFollowUpRuleCommand cmd, CancellationToken ct)
    {
        var rule = await _context.FollowUpRules
            .FirstOrDefaultAsync(r => r.Id == cmd.Id, ct);

        if (rule is null)
            return Result<FollowUpRuleDto>.NotFound($"FollowUpRule {cmd.Id} not found");

        var updateResult = rule.Update(cmd.ConsultationType, cmd.FollowUpDays, cmd.FollowUpReason);

        if (!updateResult.IsSuccess)
            return Result<FollowUpRuleDto>.Invalid(updateResult.ValidationErrors.ToList());

        await _context.SaveChangesAsync(ct);

        return Result<FollowUpRuleDto>.Success(rule.ToDto());
    }
}
