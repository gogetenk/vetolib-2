using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.SubmitVisitFeedback;

internal record SubmitVisitFeedbackCommand(
    Guid AppointmentId,
    int Rating,
    string? Comment,
    bool IsPublic) : IRequest<Result<VisitFeedbackDto>>;
