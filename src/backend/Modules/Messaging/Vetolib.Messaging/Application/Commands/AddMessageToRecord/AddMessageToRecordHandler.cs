using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.AddMessageToRecord;

internal class AddMessageToRecordHandler : IRequestHandler<AddMessageToRecordCommand, Result>
{
    private readonly MessagingDbContext _context;
    private readonly IClinicContext _clinicContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AddMessageToRecordHandler(
        MessagingDbContext context,
        IClinicContext clinicContext,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _clinicContext = clinicContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(AddMessageToRecordCommand cmd, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result.NotFound($"Conversation {cmd.ConversationId} not found");

        if (conversation.PatientId is null)
            return Result.Error("MISSING_PATIENT:This conversation is not linked to a patient");

        var message = conversation.Messages.FirstOrDefault(m => m.Id == cmd.MessageId);

        if (message is null)
            return Result.NotFound($"Message {cmd.MessageId} not found in conversation {cmd.ConversationId}");

        if (message.IsInternalNote)
            return Result.Error("INTERNAL_NOTE_NOT_ALLOWED:Internal notes cannot be attached to a medical record");

        // Resolve attachments for this message
        var attachments = await _context.MessageAttachments
            .Where(a => a.MessageId == cmd.MessageId)
            .AsNoTracking()
            .ToListAsync(ct);

        var attachmentPaths = attachments.Select(a => a.StoragePath).ToList();

        await _publishEndpoint.Publish(new MessageAttachedToRecordEvent(
            ConversationId: conversation.Id,
            MessageId: message.Id,
            PatientId: conversation.PatientId.Value,
            ClinicId: _clinicContext.ClinicId,
            MessageBody: message.Body,
            AttachmentStoragePaths: attachmentPaths,
            AttachedAt: DateTime.UtcNow), ct);

        return Result.Success();
    }
}
