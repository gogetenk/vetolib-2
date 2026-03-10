using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.SendOwnerMessage;

internal class SendOwnerMessageHandler : IRequestHandler<SendOwnerMessageCommand, Result<Guid>>
{
    private const int DailyMessageLimit = 5;

    private readonly MessagingDbContext _context;
    private readonly ITriageOrchestrator _triageOrchestrator;

    public SendOwnerMessageHandler(MessagingDbContext context, ITriageOrchestrator triageOrchestrator)
    {
        _context = context;
        _triageOrchestrator = triageOrchestrator;
    }

    public async Task<Result<Guid>> Handle(
        SendOwnerMessageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load the conversation and verify ownership
        var conversation = await _context.Conversations
            .IgnoreQueryFilters()
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(
                c => c.Id == request.ConversationId
                  && c.OwnerId == request.OwnerId
                  && c.ClinicId == request.ClinicId,
                cancellationToken);

        if (conversation is null)
            return Result<Guid>.NotFound("Conversation not found");

        if (conversation.Status == ConversationStatus.Closed)
            return Result<Guid>.Error("CONVERSATION_CLOSED:This conversation has been closed. No new messages can be added.");

        // 2. Check daily message limit (5 messages/day/owner/clinic)
        var todayUtc = DateTime.UtcNow.Date;
        var ownerConversationIds = await _context.Conversations
            .IgnoreQueryFilters()
            .Where(c => c.OwnerId == request.OwnerId && c.ClinicId == request.ClinicId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var messageCountToday = await _context.Messages
            .IgnoreQueryFilters()
            .CountAsync(
                m => m.Sender == MessageSender.Owner
                  && m.SentAt >= todayUtc
                  && ownerConversationIds.Contains(m.ConversationId),
                cancellationToken);

        if (messageCountToday >= DailyMessageLimit)
            return Result<Guid>.Error("DAILY_LIMIT_EXCEEDED:You have reached the daily message limit. Please try again tomorrow.");

        // 3. If conversation was Resolved, reopen it
        if (conversation.Status == ConversationStatus.Resolved)
        {
            conversation.Reopen();
        }

        // 4. Add the owner message
        var messageResult = conversation.AddMessage(MessageSender.Owner, null, request.Body);
        if (!messageResult.IsSuccess)
            return Result<Guid>.Error(string.Join("; ", messageResult.Errors));

        // 5. Re-triage based on the new message content (falls back gracefully)
        await _triageOrchestrator.ApplyTriageAsync(conversation, request.Body, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(messageResult.Value.Id);
    }
}
