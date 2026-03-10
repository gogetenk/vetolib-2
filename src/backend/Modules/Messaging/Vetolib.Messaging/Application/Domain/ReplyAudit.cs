using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class ReplyAudit : BaseEntity
{
    public Guid MessageId { get; private set; }
    public Guid OriginalOwnerMessageId { get; private set; }
    public string? AiSuggestedReply { get; private set; }
    public bool WasSuggestedReplyUsed { get; private set; }
    public string ActualReply { get; private set; } = string.Empty;

    private ReplyAudit() { } // EF Core

    public static Result<ReplyAudit> Create(
        Guid messageId,
        Guid originalOwnerMessageId,
        string actualReply,
        string? aiSuggestedReply = null,
        bool wasSuggestedReplyUsed = false)
    {
        var errors = new List<ValidationError>();

        if (messageId == Guid.Empty)
            errors.Add(new ValidationError(nameof(messageId), "MessageId is required"));

        if (originalOwnerMessageId == Guid.Empty)
            errors.Add(new ValidationError(nameof(originalOwnerMessageId), "OriginalOwnerMessageId is required"));

        if (string.IsNullOrWhiteSpace(actualReply))
            errors.Add(new ValidationError(nameof(actualReply), "ActualReply is required"));

        if (errors.Count > 0)
            return Result<ReplyAudit>.Invalid(errors);

        return Result<ReplyAudit>.Success(new ReplyAudit
        {
            MessageId = messageId,
            OriginalOwnerMessageId = originalOwnerMessageId,
            AiSuggestedReply = aiSuggestedReply,
            WasSuggestedReplyUsed = wasSuggestedReplyUsed,
            ActualReply = actualReply
        });
    }

    public ReplyAuditDto ToDto() => new(
        Id,
        MessageId,
        OriginalOwnerMessageId,
        AiSuggestedReply,
        WasSuggestedReplyUsed,
        ActualReply);
}
