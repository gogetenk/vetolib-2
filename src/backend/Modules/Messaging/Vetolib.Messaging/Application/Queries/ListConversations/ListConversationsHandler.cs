using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.ListConversations;

internal class ListConversationsHandler : IRequestHandler<ListConversationsQuery, Result<IReadOnlyList<ConversationDto>>>
{
    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Categories visible per role
    private static readonly MessageCategory[] ReceptionistCategories =
    [
        MessageCategory.AppointmentRequest,
        MessageCategory.Administrative,
        MessageCategory.Other
    ];

    private static readonly MessageCategory[] VetCategories =
    [
        MessageCategory.MedicalUrgency,
        MessageCategory.PostOperativeFollowUp,
        MessageCategory.MedicalQuestion
    ];

    // Assistants see non-medical (same as receptionist) read-only
    private static readonly MessageCategory[] AssistantCategories =
    [
        MessageCategory.AppointmentRequest,
        MessageCategory.Administrative,
        MessageCategory.Other,
        MessageCategory.Feedback
    ];

    public ListConversationsHandler(MessagingDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<IReadOnlyList<ConversationDto>>> Handle(ListConversationsQuery query, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var q = _context.Conversations
            .Where(c => !c.IsSpam)
            .AsQueryable();

        // Role-based filtering
        if (role == "Admin")
        {
            // Admin sees everything
        }
        else if (role == "Vet")
        {
            q = q.Where(c => VetCategories.Contains(c.Category));
        }
        else if (role == "Receptionist")
        {
            q = q.Where(c => ReceptionistCategories.Contains(c.Category));
        }
        else if (role == "Assistant")
        {
            q = q.Where(c => AssistantCategories.Contains(c.Category));
        }
        else
        {
            // Unknown role: return empty
            return Result<IReadOnlyList<ConversationDto>>.Success(Array.Empty<ConversationDto>());
        }

        // Optional filters
        if (query.Status.HasValue)
            q = q.Where(c => c.Status == query.Status.Value);

        if (query.Category.HasValue)
            q = q.Where(c => c.Category == query.Category.Value);

        if (query.FromDate.HasValue)
            q = q.Where(c => c.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(c => c.CreatedAt <= query.ToDate.Value);

        // Sort by priority (Critical > High > Normal > Low) then by LastMessageAt desc
        var priorityOrder = new Dictionary<MessageCategory, int>
        {
            { MessageCategory.MedicalUrgency, 0 },
            { MessageCategory.PostOperativeFollowUp, 1 },
            { MessageCategory.MedicalQuestion, 2 },
            { MessageCategory.AppointmentRequest, 2 },
            { MessageCategory.Administrative, 3 },
            { MessageCategory.Feedback, 3 },
            { MessageCategory.Other, 3 }
        };

        var conversations = await q
            .Include(c => c.Messages)
            .ToListAsync(ct);

        var sorted = conversations
            .OrderBy(c => priorityOrder.TryGetValue(c.Category, out var p) ? p : 99)
            .ThenByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => c.ToDto())
            .ToList();

        return Result<IReadOnlyList<ConversationDto>>.Success(sorted);
    }
}
