using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.AddInternalNote;

internal record AddInternalNoteCommand(
    Guid ConversationId,
    string Body
) : IRequest<Result<MessageDto>>;
