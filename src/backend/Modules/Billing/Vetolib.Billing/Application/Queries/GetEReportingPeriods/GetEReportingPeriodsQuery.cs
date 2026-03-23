using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.GetEReportingPeriods;

internal record GetEReportingPeriodsQuery() : IRequest<Result<IReadOnlyList<EReportingPeriodDto>>>;
