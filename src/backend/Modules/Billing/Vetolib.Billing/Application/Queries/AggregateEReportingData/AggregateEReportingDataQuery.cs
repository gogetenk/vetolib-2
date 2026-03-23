using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.AggregateEReportingData;

internal record AggregateEReportingDataQuery(
    DateOnly PeriodStart,
    DateOnly PeriodEnd) : IRequest<Result<EReportingPeriodDto>>;
