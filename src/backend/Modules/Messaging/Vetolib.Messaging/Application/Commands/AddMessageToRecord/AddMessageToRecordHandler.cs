using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.AddMessageToRecord;

internal class AddMessageToRecordHandler : IRequestHandler<AddMessageToRecordCommand, Result>
{
    private readonly MessagingDbContext _context;
    private readonly IPatientRecordWriter _recordWriter;

    public AddMessageToRecordHandler(
        MessagingDbContext context,
        IPatientRecordWriter recordWriter)
    {
        _context = context;
        _recordWriter = recordWriter;
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

        var request = new AddMessageToRecordRequest(
            ConversationId: conversation.Id,
            MessageId: message.Id,
            PatientId: conversation.PatientId.Value,
            MessageBody: message.Body,
            AttachmentUrls: attachmentPaths);

        var result = await _recordWriter.AttachMessageNoteAsync(request, ct);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                return Result.NotFound(string.Join("; ", result.Errors));

            return Result.Error(string.Join("; ", result.Errors));
        }

        return Result.Success();
    }
}
