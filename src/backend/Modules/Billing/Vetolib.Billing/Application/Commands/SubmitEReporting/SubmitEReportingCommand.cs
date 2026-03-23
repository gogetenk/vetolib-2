using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.SubmitEReporting;

internal record SubmitEReportingCommand(
    DateOnly PeriodStart,
    DateOnly PeriodEnd) : IRequest<Result<EReportingPeriodDto>>;
