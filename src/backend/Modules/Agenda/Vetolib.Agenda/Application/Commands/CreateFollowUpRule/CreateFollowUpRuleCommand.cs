using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CreateFollowUpRule;

internal record CreateFollowUpRuleCommand(
    Guid ClinicId,
    string ConsultationType,
    int FollowUpDays,
    string FollowUpReason) : IRequest<Result<FollowUpRuleDto>>;
