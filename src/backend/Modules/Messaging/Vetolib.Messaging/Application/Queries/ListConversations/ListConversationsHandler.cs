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

        // When explicitly filtering for Spam, show spam conversations; otherwise exclude them
        var showSpam = query.Status == ConversationStatus.Spam;

        var q = _context.Conversations
            .Where(c => showSpam ? c.IsSpam : !c.IsSpam)
            .AsQueryable();

        // Role-based filtering
        if (role == "Admin")
        {
            // Admin sees everything
        }
        else if (role == "Vet")
        {
            q = q.Where(c => VetCategories.Contains(c.Category)
                || c.AssignedToRole == "Vet");
            q = q.Where(c => c.AssignedToRole == null || c.AssignedToRole == "Vet");
        }
        else if (role == "Receptionist")
        {
            q = q.Where(c => ReceptionistCategories.Contains(c.Category));
            q = q.Where(c => c.AssignedToRole == null || c.AssignedToRole == "Receptionist");
        }
        else if (role == "Assistant")
        {
            q = q.Where(c => AssistantCategories.Contains(c.Category));
            q = q.Where(c => c.AssignedToRole == null || c.AssignedToRole == "Assistant");
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

        // Sort by priority (MedicalUrgency=0 > PostOperativeFollowUp=1 > MedicalQuestion/AppointmentRequest=2 > rest=3)
        // then by LastMessageAt desc — ORDER BY / OFFSET / FETCH executed SQL-side before materialisation
        var page = await q
            .Include(c => c.Messages)
            .AsNoTracking()
            .OrderBy(c =>
                c.Category == MessageCategory.MedicalUrgency ? 0
                : c.Category == MessageCategory.PostOperativeFollowUp ? 1
                : c.Category == MessageCategory.MedicalQuestion || c.Category == MessageCategory.AppointmentRequest ? 2
                : 3)
            .ThenBy(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        // ToDto() is a domain method — projected in-memory after SQL pagination
        var dtos = page.Select(c => c.ToDto()).ToList();

        return Result<IReadOnlyList<ConversationDto>>.Success(dtos);
    }
}
