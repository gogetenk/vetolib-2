using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Vetolib.AI.Contracts;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetConversationById;

internal class GetConversationByIdHandler : IRequestHandler<GetConversationByIdQuery, Result<ConversationWithMessagesDto>>
{
    private static readonly MessageCategory[] MedicalCategories =
    [
        MessageCategory.MedicalUrgency,
        MessageCategory.PostOperativeFollowUp,
        MessageCategory.MedicalQuestion
    ];

    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMessageTriageService? _triageService;
    private readonly ILogger<GetConversationByIdHandler> _logger;

    public GetConversationByIdHandler(
        MessagingDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<GetConversationByIdHandler> logger,
        IMessageTriageService? triageService = null)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _triageService = triageService;
    }

    public async Task<Result<ConversationWithMessagesDto>> Handle(GetConversationByIdQuery query, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == query.ConversationId, ct);

        if (conversation is null)
            return Result<ConversationWithMessagesDto>.NotFound();

        // RBAC check: can this role see this category?
        var isMedical = MedicalCategories.Contains(conversation.Category);

        if (role == "Receptionist" && isMedical)
            return Result<ConversationWithMessagesDto>.Forbidden();

        if (role == "Assistant" && isMedical)
            return Result<ConversationWithMessagesDto>.Forbidden();

        // Assistants do not see internal notes
        var includeInternalNotes = role != "Assistant";

        // Generate transient AI suggestions if available
        IReadOnlyList<string> suggestedReplies = Array.Empty<string>();
        if (_triageService is not null)
        {
            var lastOwnerMessage = conversation.Messages
                .Where(m => m.Sender == MessageSender.Owner)
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.Body)
                .FirstOrDefault();

            if (lastOwnerMessage is not null)
            {
                try
                {
                    suggestedReplies = await _triageService.GenerateSuggestedRepliesAsync(
                        conversation.Subject,
                        lastOwnerMessage,
                        conversation.Category.ToString(),
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate AI suggestions for conversation {ConversationId}", conversation.Id);
                    // Fallback: return empty suggestions
                }
            }
        }

        return Result<ConversationWithMessagesDto>.Success(
            conversation.ToDetailDto(includeInternalNotes, suggestedReplies));
    }
}
