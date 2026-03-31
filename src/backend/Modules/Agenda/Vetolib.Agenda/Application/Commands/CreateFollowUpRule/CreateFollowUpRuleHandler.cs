using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CreateFollowUpRule;

internal class CreateFollowUpRuleHandler : IRequestHandler<CreateFollowUpRuleCommand, Result<FollowUpRuleDto>>
{
    private readonly AgendaDbContext _context;

    public CreateFollowUpRuleHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FollowUpRuleDto>> Handle(CreateFollowUpRuleCommand cmd, CancellationToken ct)
    {
        var duplicateExists = await _context.FollowUpRules
            .AnyAsync(r => r.ConsultationType == cmd.ConsultationType && r.IsActive, ct);

        if (duplicateExists)
            return Result<FollowUpRuleDto>.Conflict(
                $"An active follow-up rule for consultation type '{cmd.ConsultationType}' already exists.");

        var createResult = FollowUpRule.Create(
            cmd.ClinicId,
            cmd.ConsultationType,
            cmd.FollowUpDays,
            cmd.FollowUpReason);

        if (!createResult.IsSuccess)
            return createResult.Map(_ => (FollowUpRuleDto)null!);

        _context.FollowUpRules.Add(createResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<FollowUpRuleDto>.Success(createResult.Value.ToDto());
    }
}
