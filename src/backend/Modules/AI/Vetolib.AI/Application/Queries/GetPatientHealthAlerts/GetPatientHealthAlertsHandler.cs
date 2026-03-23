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

        var alerts = await _context.HealthAlerts
            .Where(a => a.PatientId == query.PatientId)
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.GeneratedAt)
            .Select(a => GetHealthAlertsHandler.MapToDto(a))
            .ToListAsync(ct);

        return Result<IReadOnlyList<HealthAlertDto>>.Success(alerts);
    }
}
