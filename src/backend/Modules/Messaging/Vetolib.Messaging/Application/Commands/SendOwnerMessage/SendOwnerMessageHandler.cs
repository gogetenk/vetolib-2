using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.SendOwnerMessage;

internal class SendOwnerMessageHandler : IRequestHandler<SendOwnerMessageCommand, Result<Guid>>
{
    private const int DailyMessageLimit = 5;

    private readonly MessagingDbContext _context;
    private readonly ITriageOrchestrator _triageOrchestrator;
    private readonly IMessageClassifier _classifier;
    private readonly IPublisher _publisher;
    private readonly ILogger<SendOwnerMessageHandler> _logger;

    public SendOwnerMessageHandler(
        MessagingDbContext context,
        ITriageOrchestrator triageOrchestrator,
        IMessageClassifier classifier,
        IPublisher publisher,
        ILogger<SendOwnerMessageHandler> logger)
    {
        _context = context;
        _triageOrchestrator = triageOrchestrator;
        _classifier = classifier;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        SendOwnerMessageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load the conversation and verify ownership
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(
                c => c.Id == request.ConversationId
                  && c.OwnerId == request.OwnerId,
                cancellationToken);

        if (conversation is null)
            return Result<Guid>.NotFound("Conversation not found");

        if (conversation.Status == ConversationStatus.Closed)
            return Result<Guid>.Error("CONVERSATION_CLOSED:This conversation has been closed. No new messages can be added.");

        // 2. Check daily message limit (5 messages/day/owner/clinic)
        var todayUtc = DateTime.UtcNow.Date;
        var ownerConversationIds = await _context.Conversations
            .Where(c => c.OwnerId == request.OwnerId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var messageCountToday = await _context.Messages
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

        // 6. Classify the message (AI classification is fire-and-forget safe)
        var message = messageResult.Value;
        try
        {
            var classification = await _classifier.ClassifyAsync(
                request.Body, conversation.Subject, cancellationToken);

            if (classification is not null)
            {
                var flagForReview = classification.Confidence < 0.6;
                message.ApplyClassification(
                    classification.Urgency,
                    classification.Category,
                    classification.Confidence,
                    flagForReview);

                // Publish urgent classification event
                if (classification.Urgency == ClassifiedUrgency.Critical)
                {
                    var preview = request.Body.Length > 100 ? request.Body[..100] + "..." : request.Body;
                    await _publisher.Publish(new UrgentMessageClassifiedEvent(
                        message.Id,
                        conversation.Id,
                        conversation.ClinicId,
                        classification.Urgency,
                        preview), cancellationToken);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Message classification failed for message {MessageId}. Continuing without classification.", message.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(message.Id);
    }
}
