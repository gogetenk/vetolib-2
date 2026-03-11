using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;
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
    private readonly IPatientReader _patientReader;
    private readonly IMessageTriageService? _triageService;
    private readonly ILogger<GetConversationByIdHandler> _logger;

    public GetConversationByIdHandler(
        MessagingDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IPatientReader patientReader,
        ILogger<GetConversationByIdHandler> logger,
        IMessageTriageService? triageService = null)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _patientReader = patientReader;
        _logger = logger;
        _triageService = triageService;
    }

    public async Task<Result<ConversationWithMessagesDto>> Handle(GetConversationByIdQuery query, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var conversation = await _context.Conversations
            .AsNoTracking()
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

        // Load patient context when the conversation is linked to a patient.
        // Receptionist sees basic info only (includeFullMedicalContext = false).
        // Vet/Admin sees full medical context.
        PatientContextDto? patientContext = null;
        if (conversation.PatientId.HasValue)
        {
            var includeFullMedical = role is "Vet" or "Admin";
            try
            {
                var contextResult = await _patientReader.GetPatientContextAsync(
                    conversation.PatientId.Value,
                    includeFullMedical,
                    ct);

                if (contextResult.IsSuccess)
                    patientContext = contextResult.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load patient context for conversation {ConversationId}", conversation.Id);
                // Non-blocking: conversation detail is still returned without patient context
            }
        }

        return Result<ConversationWithMessagesDto>.Success(
            conversation.ToDetailDto(includeInternalNotes, suggestedReplies, patientContext));
    }
}
