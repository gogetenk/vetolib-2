using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListFollowUpRules;

internal class ListFollowUpRulesHandler : IRequestHandler<ListFollowUpRulesQuery, Result<List<FollowUpRuleDto>>>
{
    private readonly AgendaDbContext _context;

    public ListFollowUpRulesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FollowUpRuleDto>>> Handle(ListFollowUpRulesQuery query, CancellationToken ct)
    {
        var rules = await _context.FollowUpRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.ConsultationType)
            .Select(r => new FollowUpRuleDto(
                r.Id,
                r.ConsultationType,
                r.FollowUpDays,
                r.FollowUpReason,
                r.IsActive))
            .ToListAsync(ct);

        return Result<List<FollowUpRuleDto>>.Success(rules);
    }
}
