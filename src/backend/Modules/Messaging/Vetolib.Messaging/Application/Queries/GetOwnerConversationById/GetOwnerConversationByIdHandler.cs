using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetOwnerConversationById;

internal class GetOwnerConversationByIdHandler
    : IRequestHandler<GetOwnerConversationByIdQuery, Result<ConversationWithMessagesDto>>
{
    private readonly MessagingDbContext _context;

    public GetOwnerConversationByIdHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConversationWithMessagesDto>> Handle(
        GetOwnerConversationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .IgnoreQueryFilters()
            .Include(c => c.Messages.Where(m => !m.IsInternalNote)) // Never return internal notes to owner
            .FirstOrDefaultAsync(
                c => c.Id == request.ConversationId
                  && c.OwnerId == request.OwnerId
                  && c.ClinicId == request.ClinicId,
                cancellationToken);

        if (conversation is null)
            return Result<ConversationWithMessagesDto>.NotFound("Conversation not found");

        var dto = new ConversationWithMessagesDto(
            conversation.Id,
            conversation.ClinicId,
            conversation.OwnerId,
            conversation.PatientId,
            conversation.Subject,
            conversation.Category,
            conversation.Status,
            conversation.AssignedToUserId,
            conversation.AssignedToRole,
            conversation.AiTriageConfidence,
            conversation.IsTriageUncertain,
            conversation.CreatedAt,
            conversation.LastMessageAt,
            conversation.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => m.ToDto())
                .ToList());

        return Result<ConversationWithMessagesDto>.Success(dto);
    }
}
