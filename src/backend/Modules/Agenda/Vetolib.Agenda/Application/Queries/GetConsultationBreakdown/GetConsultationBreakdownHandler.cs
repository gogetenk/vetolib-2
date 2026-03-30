using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetConsultationBreakdown;

internal class GetConsultationBreakdownHandler
    : IRequestHandler<GetConsultationBreakdownQuery, Result<IReadOnlyList<ConsultationBreakdownDto>>>
{
    private readonly AgendaDbContext _context;

    public GetConsultationBreakdownHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ConsultationBreakdownDto>>> Handle(
        GetConsultationBreakdownQuery query, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateOnly(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var rows = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= startOfMonth && a.Date < endOfMonth)
            .GroupBy(a => a.Reason ?? "Unspecified")
            .Select(g => new { ConsultationType = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync(ct);

        var result = rows
            .Select(r => new ConsultationBreakdownDto(
                ConsultationType: r.ConsultationType,
                Count: r.Count))
            .ToList();

        return Result<IReadOnlyList<ConsultationBreakdownDto>>.Success(result);
    }
}
