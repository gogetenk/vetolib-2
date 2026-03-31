using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Application.Commands.DeactivateFollowUpRule;

internal record DeactivateFollowUpRuleCommand(Guid Id) : IRequest<Result>;
