using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.ClassificationFeedback;

internal record ClassificationFeedbackCommand(
    Guid ConversationId,
    Guid MessageId,
    bool IsCorrect
) : IRequest<Result>;
