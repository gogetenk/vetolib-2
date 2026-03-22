using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.OverrideClassification;

internal record OverrideClassificationCommand(
    Guid ConversationId,
    Guid MessageId,
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category
) : IRequest<Result>;
