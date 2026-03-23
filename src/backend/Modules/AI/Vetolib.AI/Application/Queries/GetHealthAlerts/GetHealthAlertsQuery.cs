using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Queries.GetHealthAlerts;

internal record GetHealthAlertsQuery(
    HealthAlertSeverity? Severity,
    HealthAlertStatus? Status,
    Guid? PatientId) : IRequest<Result<IReadOnlyList<HealthAlertDto>>>;
