using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetVisitFeedbackStats;

internal record GetVisitFeedbackStatsQuery() : IRequest<Result<VisitFeedbackStatsDto>>;
