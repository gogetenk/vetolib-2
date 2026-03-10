using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.CreateOutboundConversation;

internal class CreateOutboundConversationHandler : IRequestHandler<CreateOutboundConversationCommand, Result<ConversationDto>>
{
    private readonly MessagingDbContext _context;
    private readonly IClinicContext _clinicContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOutboundConversationHandler(
        MessagingDbContext context,
        IClinicContext clinicContext,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _clinicContext = clinicContext;
        _httpContextAccessor = httpContextAccessor;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<ConversationDto>> Handle(CreateOutboundConversationCommand cmd, CancellationToken ct)
    {
        var clinicId = _clinicContext.ClinicId;

        var conversationResult = Conversation.Create(
            clinicId,
            cmd.OwnerId,
            cmd.PatientId,
            cmd.Subject,
            cmd.Category);

        if (!conversationResult.IsSuccess)
            return Result<ConversationDto>.Invalid(conversationResult.ValidationErrors.ToList());

        var conversation = conversationResult.Value;

        // Get admin user id for the initial message
        var user = _httpContextAccessor.HttpContext?.User;
        Guid? senderUserId = null;
        var subClaim = user?.FindFirst("sub")?.Value
            ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(subClaim, out var parsedId))
            senderUserId = parsedId;

        var messageResult = conversation.AddMessage(MessageSender.Staff, senderUserId, cmd.InitialMessageBody);

        if (!messageResult.IsSuccess)
            return Result<ConversationDto>.Error(string.Join("; ", messageResult.Errors));

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(ct);

        var preview = cmd.InitialMessageBody.Length > 100
            ? cmd.InitialMessageBody[..100] + "..."
            : cmd.InitialMessageBody;

        await _publishEndpoint.Publish(new OutboundConversationCreatedEvent(
            conversation.Id,
            cmd.OwnerId,
            clinicId,
            preview), ct);

        return Result<ConversationDto>.Success(conversation.ToDto());
    }
}
