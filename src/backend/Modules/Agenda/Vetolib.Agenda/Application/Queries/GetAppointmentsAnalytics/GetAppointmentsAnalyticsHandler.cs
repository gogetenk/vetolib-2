using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetAppointmentsAnalytics;

internal class GetAppointmentsAnalyticsHandler
    : IRequestHandler<GetAppointmentsAnalyticsQuery, Result<AppointmentsAnalyticsDto>>
{
    private readonly AgendaDbContext _context;

    public GetAppointmentsAnalyticsHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentsAnalyticsDto>> Handle(
        GetAppointmentsAnalyticsQuery query, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        var statusGroups = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= cutoff)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total = statusGroups.Sum(g => g.Count);

        // No-show rate: CANCELLED or NOSHOW as a share of all appointments last 30 days
        var noShowCount = statusGroups
            .Where(g => g.Status == AppointmentStatus.Cancelled || g.Status == AppointmentStatus.NoShow)
            .Sum(g => g.Count);

        var noShowRate = total > 0
            ? Math.Round((decimal)noShowCount / total * 100, 1)
            : 0m;

        var byStatus = statusGroups
            .Select(g => new AppointmentStatusCountDto(
                Status: g.Status.ToString().ToUpperInvariant(),
                Count: g.Count))
            .OrderByDescending(x => x.Count)
            .ToList();

        return Result<AppointmentsAnalyticsDto>.Success(
            new AppointmentsAnalyticsDto(noShowRate, byStatus));
    }
}
