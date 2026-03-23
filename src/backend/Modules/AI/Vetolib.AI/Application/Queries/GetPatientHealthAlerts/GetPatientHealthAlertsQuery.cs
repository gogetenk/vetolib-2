using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Queries.GetPatientHealthAlerts;

internal record GetPatientHealthAlertsQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<HealthAlertDto>>>;
