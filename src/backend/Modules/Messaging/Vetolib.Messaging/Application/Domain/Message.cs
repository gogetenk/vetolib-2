using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class Message : BaseEntity
{
    private const int MaxAttachmentsPerMessage = 3;

    public Guid ConversationId { get; private set; }
    public MessageSender Sender { get; private set; }
    public Guid? SenderUserId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public bool IsInternalNote { get; private set; }
    public DateTime SentAt { get; private set; }

    // Classification AI
    public ClassifiedUrgency? ClassifiedUrgency { get; private set; }
    public ClassifiedCategory? ClassifiedCategory { get; private set; }
    public double? ClassifiedConfidence { get; private set; }
    public bool IsFlaggedForReview { get; private set; }

    // Override vet
    public Guid? OverriddenByUserId { get; private set; }
    public ClassifiedUrgency? OriginalAiUrgency { get; private set; }
    public ClassifiedCategory? OriginalAiCategory { get; private set; }

    // Feedback
    public bool? ClassificationFeedbackCorrect { get; private set; }

    private readonly List<MessageAttachment> _attachments = [];
    public IReadOnlyList<MessageAttachment> Attachments => _attachments.AsReadOnly();

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

    /// <summary>
    /// Adds an attachment to this message. Enforces max 3 attachments per message (spec section 2.7).
    /// </summary>
    public Result<MessageAttachment> AddAttachment(
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath)
    {
        if (_attachments.Count >= MaxAttachmentsPerMessage)
            return Result<MessageAttachment>.Error($"ATTACHMENT_LIMIT:A message cannot have more than {MaxAttachmentsPerMessage} attachments");

        var attachmentResult = MessageAttachment.Create(Id, fileName, contentType, fileSizeBytes, storagePath);
        if (!attachmentResult.IsSuccess)
            return attachmentResult;

        _attachments.Add(attachmentResult.Value);
        return attachmentResult;
    }

    /// <summary>
    /// Applies AI classification result. Called once when message is created.
    /// </summary>
    public Result ApplyClassification(
        ClassifiedUrgency urgency,
        ClassifiedCategory category,
        double confidence,
        bool flagForReview)
    {
        if (confidence is < 0 or > 1)
            return Result.Error("INVALID_CONFIDENCE:Confidence must be between 0 and 1");

        ClassifiedUrgency = urgency;
        ClassifiedCategory = category;
        ClassifiedConfidence = confidence;
        IsFlaggedForReview = flagForReview;
        return Result.Success();
    }

    /// <summary>
    /// Vet overrides the AI classification. Original AI values are preserved.
    /// </summary>
    public Result OverrideClassification(
        Guid userId,
        ClassifiedUrgency newUrgency,
        ClassifiedCategory newCategory)
    {
        if (ClassifiedUrgency is null)
            return Result.Error("NOT_CLASSIFIED:Message has not been classified yet");

        // Preserve original AI values (only on first override)
        if (OriginalAiUrgency is null)
        {
            OriginalAiUrgency = ClassifiedUrgency;
            OriginalAiCategory = ClassifiedCategory;
        }

        OverriddenByUserId = userId;
        ClassifiedUrgency = newUrgency;
        ClassifiedCategory = newCategory;
        IsFlaggedForReview = false; // override resolves review flag
        return Result.Success();
    }

    /// <summary>
    /// Staff confirms or corrects the classification (feedback for training).
    /// </summary>
    public Result RecordClassificationFeedback(bool isCorrect)
    {
        if (ClassifiedUrgency is null)
            return Result.Error("NOT_CLASSIFIED:Message has not been classified yet");

        ClassificationFeedbackCorrect = isCorrect;
        return Result.Success();
    }

    public MessageDto ToDto() => new(
        Id,
        ConversationId,
        Sender,
        SenderUserId,
        Body,
        IsInternalNote,
        SentAt,
        ClassifiedUrgency is not null
            ? new MessageClassificationDto(
                ClassifiedUrgency.Value,
                ClassifiedCategory!.Value,
                ClassifiedConfidence!.Value,
                IsFlaggedForReview,
                OverriddenByUserId,
                OriginalAiUrgency,
                OriginalAiCategory)
            : null);
}
