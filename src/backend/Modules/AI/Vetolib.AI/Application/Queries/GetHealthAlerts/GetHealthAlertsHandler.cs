using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Queries.GetHealthAlerts;

internal class GetHealthAlertsHandler : IRequestHandler<GetHealthAlertsQuery, Result<IReadOnlyList<HealthAlertDto>>>
{
    private readonly AIDbContext _context;

    public GetHealthAlertsHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<HealthAlertDto>>> Handle(
        GetHealthAlertsQuery query,
        CancellationToken ct)
    {
        var q = _context.HealthAlerts
            .Where(a => a.Status != HealthAlertStatus.Dismissed)
            .AsQueryable();

        if (query.Severity.HasValue)
            q = q.Where(a => a.Severity == query.Severity.Value);

        if (query.Status.HasValue)
            q = q.Where(a => a.Status == query.Status.Value);

        if (query.PatientId.HasValue)
            q = q.Where(a => a.PatientId == query.PatientId.Value);

        var alerts = await q
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.GeneratedAt)
            .Select(a => MapToDto(a))
            .ToListAsync(ct);

        return Result<IReadOnlyList<HealthAlertDto>>.Success(alerts);
    }

    internal static HealthAlertDto MapToDto(HealthAlert a) => new(
        a.Id,
        a.PatientId,
        a.AlertType,
        a.Severity,
        a.Title,
        a.Description,
        a.RecommendedAction,
        a.RuleId,
        a.RiskScore,
        a.Status,
        a.GeneratedAt,
        a.DismissedAt,
        a.DismissedReason,
        a.DismissedByName,
        a.AcknowledgedAt,
        a.ConvertedToAppointmentId);
}
