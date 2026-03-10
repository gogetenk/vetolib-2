using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class Message : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public MessageSender Sender { get; private set; }
    public Guid? SenderUserId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public bool IsInternalNote { get; private set; }
    public DateTime SentAt { get; private set; }

    private Message() { } // EF Core

    public static Result<Message> Create(
        Guid conversationId,
        MessageSender sender,
        Guid? senderUserId,
        string body,
        bool isInternalNote = false)
    {
        var errors = new List<ValidationError>();

        if (conversationId == Guid.Empty)
            errors.Add(new ValidationError(nameof(conversationId), "ConversationId is required"));

        if (string.IsNullOrWhiteSpace(body))
            errors.Add(new ValidationError(nameof(body), "Message body is required"));

        if (body?.Length > 2000)
            errors.Add(new ValidationError(nameof(body), "Message body cannot exceed 2000 characters"));

        if (errors.Count > 0)
            return Result<Message>.Invalid(errors);

        return Result<Message>.Success(new Message
        {
            ConversationId = conversationId,
            Sender = sender,
            SenderUserId = senderUserId,
            Body = body!,
            IsInternalNote = isInternalNote,
            SentAt = DateTime.UtcNow
        });
    }

    public MessageDto ToDto() => new(
        Id,
        ConversationId,
        Sender,
        SenderUserId,
        Body,
        IsInternalNote,
        SentAt);
}
