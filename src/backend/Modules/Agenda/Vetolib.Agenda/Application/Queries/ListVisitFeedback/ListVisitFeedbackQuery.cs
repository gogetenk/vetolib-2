using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListVisitFeedback;

internal record ListVisitFeedbackQuery(int PageNumber = 1, int PageSize = 50)
    : IRequest<Result<List<VisitFeedbackDto>>>;
