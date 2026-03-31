using System.Text;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.ExportOwnerConversations;

internal class ExportOwnerConversationsHandler
    : IRequestHandler<ExportOwnerConversationsQuery, Result<string>>
{
    private readonly MessagingDbContext _context;

    public ExportOwnerConversationsHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        ExportOwnerConversationsQuery request,
        CancellationToken cancellationToken)
    {
        // ClinicId is handled by the global query filter via PortalAwareClinicContext.
        var conversations = await _context.Conversations
            .Include(c => c.Messages
                .Where(m => !m.IsInternalNote)
                .OrderBy(m => m.SentAt)
                .Take(100))
            .Where(c => c.OwnerId == request.OwnerId)
            .OrderBy(c => c.CreatedAt)
            .Take(500)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("VETOLIB - Conversation Export");
        sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine($"Owner ID: {request.OwnerId}");
        sb.AppendLine(new string('=', 60));
        sb.AppendLine();

        foreach (var conv in conversations)
        {
            sb.AppendLine($"Conversation: {conv.Subject}");
            sb.AppendLine($"Category: {conv.Category}");
            sb.AppendLine($"Status: {conv.Status}");
            sb.AppendLine($"Created: {conv.CreatedAt:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine(new string('-', 40));

            foreach (var msg in conv.Messages.OrderBy(m => m.SentAt))
            {
                var senderLabel = msg.Sender == MessageSender.Owner ? "You" : "Clinic";
                sb.AppendLine($"[{msg.SentAt:yyyy-MM-dd HH:mm}] {senderLabel}:");
                sb.AppendLine(msg.Body);
                sb.AppendLine();
            }

            sb.AppendLine(new string('=', 60));
            sb.AppendLine();
        }

        return Result<string>.Success(sb.ToString());
    }
}
