using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.SendReply;

internal record SendReplyCommand(
    Guid ConversationId,
    string Body
) : IRequest<Result<MessageDto>>;
