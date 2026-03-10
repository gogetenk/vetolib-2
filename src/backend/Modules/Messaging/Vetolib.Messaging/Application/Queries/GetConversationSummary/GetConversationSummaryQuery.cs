using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Queries.GetConversationSummary;

internal record GetConversationSummaryQuery(Guid ConversationId) : IRequest<Result<string>>;
