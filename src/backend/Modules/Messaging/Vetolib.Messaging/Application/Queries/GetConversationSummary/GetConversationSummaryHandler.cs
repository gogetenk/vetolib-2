using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.AI.Contracts;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetConversationSummary;

/// <summary>
/// Returns AI conversation summary if the thread has more than 5 messages.
/// Calls IConversationSummaryService from AI.Contracts when available.
/// Falls back to NotFound if AI is unavailable.
/// </summary>
internal class GetConversationSummaryHandler : IRequestHandler<GetConversationSummaryQuery, Result<string>>
{
    private readonly MessagingDbContext _context;
    private readonly IConversationSummaryService? _summaryService;
    private readonly ILogger<GetConversationSummaryHandler> _logger;

    public GetConversationSummaryHandler(
        MessagingDbContext context,
        ILogger<GetConversationSummaryHandler> logger,
        IConversationSummaryService? summaryService = null)
    {
        _context = context;
        _logger = logger;
        _summaryService = summaryService;
    }

    public async Task<Result<string>> Handle(GetConversationSummaryQuery query, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == query.ConversationId, ct);

        if (conversation is null)
            return Result<string>.NotFound();

        if (conversation.Messages.Count <= 5)
            return Result<string>.NotFound("Summary is only available for conversations with more than 5 messages.");

        if (_summaryService is null)
            return Result<string>.NotFound("AI summary service is not available.");

        var messageTexts = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Where(m => !m.IsInternalNote)
            .Select(m => $"[{m.Sender}] {m.Body}")
            .ToList()
            .AsReadOnly();

        try
        {
            var summary = await _summaryService.SummarizeAsync(messageTexts, ct);

            if (summary is null)
                return Result<string>.NotFound("AI summary service returned no result.");

            return Result<string>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI summary failed for conversation {ConversationId}", conversation.Id);
            return Result<string>.NotFound("AI summary service is not available.");
        }
    }
}
