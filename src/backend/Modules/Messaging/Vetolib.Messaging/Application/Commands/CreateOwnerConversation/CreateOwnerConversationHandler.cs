using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.CreateOwnerConversation;

internal class CreateOwnerConversationHandler : IRequestHandler<CreateOwnerConversationCommand, Result<CreateOwnerConversationResponse>>
{
    private const string AutoAcknowledgmentBody =
        "Your message has been received. It will be processed when the clinic reopens.";

    private const int DailyMessageLimit = 5;

    // SLA estimates per category (business hours)
    private static readonly Dictionary<MessageCategory, string> SlaEstimates = new()
    {
        [MessageCategory.MedicalUrgency] = "15 minutes",
        [MessageCategory.PostOperativeFollowUp] = "2 hours",
        [MessageCategory.MedicalQuestion] = "8 hours",
        [MessageCategory.AppointmentRequest] = "4 hours",
        [MessageCategory.Administrative] = "24 hours",
        [MessageCategory.Feedback] = "48 hours",
        [MessageCategory.Other] = "24 hours"
    };

    private readonly MessagingDbContext _context;
    private readonly IBusinessHoursChecker _businessHoursChecker;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITriageOrchestrator _triageOrchestrator;
    private readonly IMessageRouter _router;
    private readonly IMessageClassifier _classifier;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateOwnerConversationHandler> _logger;

    public CreateOwnerConversationHandler(
        MessagingDbContext context,
        IBusinessHoursChecker businessHoursChecker,
        IPublishEndpoint publishEndpoint,
        ITriageOrchestrator triageOrchestrator,
        IMessageRouter router,
        IMessageClassifier classifier,
        IPublisher publisher,
        ILogger<CreateOwnerConversationHandler> logger)
    {
        _context = context;
        _businessHoursChecker = businessHoursChecker;
        _publishEndpoint = publishEndpoint;
        _triageOrchestrator = triageOrchestrator;
        _router = router;
        _classifier = classifier;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<CreateOwnerConversationResponse>> Handle(
        CreateOwnerConversationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify consent has been accepted
        var portalToken = await _context.OwnerPortalTokens
            .FirstOrDefaultAsync(
                t => t.OwnerId == request.OwnerId,
                cancellationToken);

        if (portalToken is null)
            return Result<CreateOwnerConversationResponse>.NotFound("Portal token not found");

        if (portalToken.ConsentAcceptedAt is null)
            return Result<CreateOwnerConversationResponse>.Forbidden();

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
            return Result<CreateOwnerConversationResponse>.Error("DAILY_LIMIT_EXCEEDED:You have reached the daily message limit. Please try again tomorrow.");

        // 3. Derive subject from body if not provided
        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? (request.Body.Length > 100 ? request.Body[..100] : request.Body)
            : request.Subject;

        // 4. Create conversation with owner-provided category as initial default
        var conversationResult = Conversation.Create(
            request.ClinicId,
            request.OwnerId,
            request.PatientId,
            subject,
            request.Category);

        if (!conversationResult.IsSuccess)
            return Result<CreateOwnerConversationResponse>.Invalid(conversationResult.ValidationErrors);

        var conversation = conversationResult.Value;

        // 5. Add owner's first message
        var messageResult = conversation.AddMessage(MessageSender.Owner, null, request.Body);
        if (!messageResult.IsSuccess)
            return Result<CreateOwnerConversationResponse>.Error(string.Join("; ", messageResult.Errors));

        // 6. AI triage — updates category, confidence, and routing (falls back gracefully)
        await _triageOrchestrator.ApplyTriageAsync(conversation, request.Body, cancellationToken);

        // 6b. Classify the first message (AI classification is fire-and-forget safe)
        try
        {
            var classification = await _classifier.ClassifyAsync(
                request.Body, subject, cancellationToken);

            if (classification is not null)
            {
                var flagForReview = classification.Confidence < 0.6;
                messageResult.Value.ApplyClassification(
                    classification.Urgency,
                    classification.Category,
                    classification.Confidence,
                    flagForReview);

                // Publish urgent classification event
                if (classification.Urgency == ClassifiedUrgency.Critical)
                {
                    var classPreview = request.Body.Length > 100 ? request.Body[..100] + "..." : request.Body;
                    await _publisher.Publish(new UrgentMessageClassifiedEvent(
                        messageResult.Value.Id,
                        conversation.Id,
                        request.ClinicId,
                        classification.Urgency,
                        classPreview), cancellationToken);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Message classification failed for new conversation. Continuing without classification.");
        }

        _context.Conversations.Add(conversation);

        // 7. Check business hours — add auto-acknowledgment for non-urgent messages outside hours
        string? acknowledgment = null;
        var isUrgent = conversation.Category == MessageCategory.MedicalUrgency;
        if (!isUrgent)
        {
            var isWithinHours = await _businessHoursChecker.IsWithinBusinessHoursAsync(
                request.ClinicId,
                DateTime.UtcNow,
                cancellationToken);

            if (!isWithinHours)
            {
                acknowledgment = AutoAcknowledgmentBody;
                conversation.AddMessage(MessageSender.System, null, AutoAcknowledgmentBody);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // 8. Publish integration event for urgent messages
        if (isUrgent)
        {
            var preview = request.Body.Length > 100 ? request.Body[..100] + "..." : request.Body;
            await _publishEndpoint.Publish(new EmergencyMessageReceivedEvent(
                conversation.Id,
                request.ClinicId,
                request.PatientId,
                preview), cancellationToken);
        }

        var estimatedResponseTime = SlaEstimates.GetValueOrDefault(conversation.Category, "24 hours");
        var assignedRole = conversation.AssignedToRole ?? _router.GetAssignedRole(conversation.Category);

        return Result<CreateOwnerConversationResponse>.Success(new CreateOwnerConversationResponse(
            conversation.Id,
            estimatedResponseTime,
            assignedRole,
            acknowledgment));
    }
}
