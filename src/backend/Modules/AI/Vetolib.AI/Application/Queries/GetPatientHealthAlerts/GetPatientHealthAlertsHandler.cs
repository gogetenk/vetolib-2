using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Application.Queries.GetHealthAlerts;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Queries.GetPatientHealthAlerts;

internal class GetPatientHealthAlertsHandler : IRequestHandler<GetPatientHealthAlertsQuery, Result<IReadOnlyList<HealthAlertDto>>>
{
    private readonly AIDbContext _context;

    public GetPatientHealthAlertsHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<HealthAlertDto>>> Handle(
        GetPatientHealthAlertsQuery query,
        CancellationToken ct)
    {
        if (query.PatientId == Guid.Empty)
            return Result<IReadOnlyList<HealthAlertDto>>.Invalid(
                new ValidationError(nameof(query.PatientId), "PatientId is required."));

        // Severity is stored as string in DB, so we cannot rely on DB-level ordering.
        // Materialize first, then sort client-side with explicit priority mapping.
        var alerts = await _context.HealthAlerts
            .Where(a => a.PatientId == query.PatientId)
            .Select(a => GetHealthAlertsHandler.MapToDto(a))
            .ToListAsync(ct);

        alerts.Sort((a, b) =>
        {
            var cmp = GetHealthAlertsHandler.SeverityPriority(b.Severity)
                .CompareTo(GetHealthAlertsHandler.SeverityPriority(a.Severity));
            return cmp != 0 ? cmp : b.GeneratedAt.CompareTo(a.GeneratedAt);
        });

        return Result<IReadOnlyList<HealthAlertDto>>.Success(alerts);
    }
}
