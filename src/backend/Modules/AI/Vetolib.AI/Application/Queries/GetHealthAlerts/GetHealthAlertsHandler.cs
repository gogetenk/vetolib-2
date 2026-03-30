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

    /// <summary>
    /// Explicit priority mapping for severity sorting. Higher value = higher priority.
    /// This avoids relying on enum integer order or alphabetical string order in the DB.
    /// </summary>
    internal static int SeverityPriority(HealthAlertSeverity severity) => severity switch
    {
        HealthAlertSeverity.High => 3,
        HealthAlertSeverity.Medium => 2,
        HealthAlertSeverity.Low => 1,
        _ => 0
    };

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

        // Severity is stored as string in DB, so we cannot rely on DB-level ordering.
        // Materialize first, then sort client-side with explicit priority mapping.
        var alerts = await q
            .Select(a => MapToDto(a))
            .ToListAsync(ct);

        alerts.Sort((a, b) =>
        {
            var cmp = SeverityPriority(b.Severity).CompareTo(SeverityPriority(a.Severity));
            return cmp != 0 ? cmp : b.GeneratedAt.CompareTo(a.GeneratedAt);
        });

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
