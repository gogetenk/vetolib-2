using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListFollowUpRules;

internal record ListFollowUpRulesQuery() : IRequest<Result<List<FollowUpRuleDto>>>;
