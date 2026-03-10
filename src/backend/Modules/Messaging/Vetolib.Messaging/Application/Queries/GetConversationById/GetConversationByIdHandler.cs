using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

    public GetConversationByIdHandler(MessagingDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
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

        return Result<ConversationWithMessagesDto>.Success(conversation.ToDetailDto(includeInternalNotes));
    }
}
