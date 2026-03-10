using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.CreateOwnerConversation;

internal class CreateOwnerConversationHandler : IRequestHandler<CreateOwnerConversationCommand, Result<Guid>>
{
    private const string AutoAcknowledgmentBody =
        "Your message has been received. It will be processed when the clinic reopens.";

    private const int DailyMessageLimit = 5;

    private readonly MessagingDbContext _context;
    private readonly IBusinessHoursChecker _businessHoursChecker;

    public CreateOwnerConversationHandler(MessagingDbContext context, IBusinessHoursChecker businessHoursChecker)
    {
        _context = context;
        _businessHoursChecker = businessHoursChecker;
    }

    public async Task<Result<Guid>> Handle(
        CreateOwnerConversationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify consent has been accepted
        var portalToken = await _context.OwnerPortalTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                t => t.OwnerId == request.OwnerId && t.ClinicId == request.ClinicId,
                cancellationToken);

        if (portalToken is null)
            return Result<Guid>.NotFound("Portal token not found");

        if (portalToken.ConsentAcceptedAt is null)
            return Result<Guid>.Error("CONSENT_REQUIRED:You must accept the messaging terms before sending a message");

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

        // 3. Create conversation
        var conversationResult = Conversation.Create(
            request.ClinicId,
            request.OwnerId,
            request.PatientId,
            request.Subject,
            request.Category);

        if (!conversationResult.IsSuccess)
            return Result<Guid>.Invalid(conversationResult.ValidationErrors);

        var conversation = conversationResult.Value;

        // 4. Add owner's first message
        var messageResult = conversation.AddMessage(MessageSender.Owner, null, request.Body);
        if (!messageResult.IsSuccess)
            return Result<Guid>.Error(string.Join("; ", messageResult.Errors));

        _context.Conversations.Add(conversation);

        // 5. Check business hours — add auto-acknowledgment for non-urgent messages outside hours
        var isUrgent = request.Category == MessageCategory.MedicalUrgency;
        if (!isUrgent)
        {
            var isWithinHours = await _businessHoursChecker.IsWithinBusinessHoursAsync(
                request.ClinicId,
                DateTime.UtcNow,
                cancellationToken);

            if (!isWithinHours)
            {
                conversation.AddMessage(MessageSender.System, null, AutoAcknowledgmentBody);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(conversation.Id);
    }
}
