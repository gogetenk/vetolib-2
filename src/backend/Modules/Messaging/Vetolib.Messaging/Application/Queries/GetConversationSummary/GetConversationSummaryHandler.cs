using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetConversationSummary;

/// <summary>
/// Returns AI conversation summary if the thread has more than 5 messages.
/// Calls IConversationSummaryService from AI.Contracts when available.
/// At MVP, the AI module summary service is not yet wired — returns NotFound.
/// </summary>
internal class GetConversationSummaryHandler : IRequestHandler<GetConversationSummaryQuery, Result<string>>
{
    private readonly MessagingDbContext _context;

    public GetConversationSummaryHandler(MessagingDbContext context)
    {
        _context = context;
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

        // IConversationSummaryService from AI.Contracts not yet available in this module.
        // Return NotFound until the AI module integration task wires this up.
        return Result<string>.NotFound("AI summary service is not yet available.");
    }
}
