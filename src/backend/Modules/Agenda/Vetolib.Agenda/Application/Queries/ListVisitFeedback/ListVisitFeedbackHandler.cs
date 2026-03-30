using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListVisitFeedback;

internal class ListVisitFeedbackHandler : IRequestHandler<ListVisitFeedbackQuery, Result<List<VisitFeedbackDto>>>
{
    private readonly AgendaDbContext _context;

    public ListVisitFeedbackHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<VisitFeedbackDto>>> Handle(ListVisitFeedbackQuery query, CancellationToken ct)
    {
        var page = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 50 : Math.Min(query.PageSize, 200);

        var feedbacks = await _context.VisitFeedbacks
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = feedbacks.Select(f => f.ToDto()).ToList();

        return Result<List<VisitFeedbackDto>>.Success(dtos);
    }
}
