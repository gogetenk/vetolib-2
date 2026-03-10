using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Queries.ExportOwnerConversations;

internal record ExportOwnerConversationsQuery(Guid OwnerId, Guid ClinicId)
    : IRequest<Result<string>>;
