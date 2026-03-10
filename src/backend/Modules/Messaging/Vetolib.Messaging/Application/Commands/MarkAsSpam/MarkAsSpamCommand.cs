using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.MarkAsSpam;

internal record MarkAsSpamCommand(Guid ConversationId) : IRequest<Result>;
