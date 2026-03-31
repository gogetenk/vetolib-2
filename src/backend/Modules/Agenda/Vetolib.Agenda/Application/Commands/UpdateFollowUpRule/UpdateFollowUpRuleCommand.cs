using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.UpdateFollowUpRule;

internal record UpdateFollowUpRuleCommand(
    Guid Id,
    string ConsultationType,
    int FollowUpDays,
    string FollowUpReason) : IRequest<Result<FollowUpRuleDto>>;
