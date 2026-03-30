using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetVisitFeedbackStats;

internal class GetVisitFeedbackStatsHandler : IRequestHandler<GetVisitFeedbackStatsQuery, Result<VisitFeedbackStatsDto>>
{
    private readonly AgendaDbContext _context;

    public GetVisitFeedbackStatsHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VisitFeedbackStatsDto>> Handle(GetVisitFeedbackStatsQuery query, CancellationToken ct)
    {
        var feedbacks = await _context.VisitFeedbacks
            .AsNoTracking()
            .ToListAsync(ct);

        if (feedbacks.Count == 0)
        {
            return Result<VisitFeedbackStatsDto>.Success(
                new VisitFeedbackStatsDto(0, 0, 0, 0, 0, 0, 0, 0));
        }

        var totalCount = feedbacks.Count;
        var averageRating = feedbacks.Average(f => f.Rating);
        var countStar1 = feedbacks.Count(f => f.Rating == 1);
        var countStar2 = feedbacks.Count(f => f.Rating == 2);
        var countStar3 = feedbacks.Count(f => f.Rating == 3);
        var countStar4 = feedbacks.Count(f => f.Rating == 4);
        var countStar5 = feedbacks.Count(f => f.Rating == 5);

        // NPS: promoters (4-5) minus detractors (1-2) as percentage
        var promoters = feedbacks.Count(f => f.Rating >= 4);
        var detractors = feedbacks.Count(f => f.Rating <= 2);
        var npsScore = ((double)(promoters - detractors) / totalCount) * 100;

        return Result<VisitFeedbackStatsDto>.Success(
            new VisitFeedbackStatsDto(
                Math.Round(averageRating, 2),
                totalCount,
                countStar1,
                countStar2,
                countStar3,
                countStar4,
                countStar5,
                Math.Round(npsScore, 2)));
    }
}
