using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.SendReply;

internal record SendReplyCommand(
    Guid ConversationId,
    string Body,
    string? AiSuggestedReply = null,
    bool WasSuggestedReplyUsed = false
) : IRequest<Result<MessageDto>>;
