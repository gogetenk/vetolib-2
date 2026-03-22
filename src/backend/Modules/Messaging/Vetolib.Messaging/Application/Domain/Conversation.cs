using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class Conversation : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid? PatientId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public MessageCategory Category { get; private set; }
    public ConversationStatus Status { get; private set; }
    public ConversationChannel Channel { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? AssignedToRole { get; private set; }
    public decimal? AiTriageConfidence { get; private set; }
    public bool IsTriageUncertain { get; private set; }
    public bool IsSpam { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public DateTime? EscalationSentAt { get; private set; }

    private readonly List<Message> _messages = [];
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

    private Conversation() { } // EF Core

    public static Result<Conversation> Create(
        Guid clinicId,
        Guid ownerId,
        Guid? patientId,
        string subject,
        MessageCategory category,
        ConversationChannel channel = ConversationChannel.Portal)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (ownerId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ownerId), "OwnerId is required"));

        if (string.IsNullOrWhiteSpace(subject))
            errors.Add(new ValidationError(nameof(subject), "Subject is required"));

        if (errors.Count > 0)
            return Result<Conversation>.Invalid(errors);

        return Result<Conversation>.Success(new Conversation
        {
            ClinicId = clinicId,
            OwnerId = ownerId,
            PatientId = patientId,
            Subject = subject,
            Category = category,
            Channel = channel,
            Status = ConversationStatus.Open
        });
    }

    public Result<Message> AddMessage(
        MessageSender sender,
        Guid? senderUserId,
        string body,
        bool isInternalNote = false)
    {
        if (Status == ConversationStatus.Closed)
            return Result<Message>.Error("CONVERSATION_CLOSED:Cannot add message to a closed conversation");

        var messageResult = Message.Create(Id, sender, senderUserId, body, isInternalNote);
        if (!messageResult.IsSuccess)
            return messageResult;

        _messages.Add(messageResult.Value);
        LastMessageAt = messageResult.Value.SentAt;

        // Spec section 2.9: owner message to a Resolved conversation reopens it automatically
        if (Status == ConversationStatus.Resolved && sender == MessageSender.Owner)
            Status = ConversationStatus.Open;

        if (Status == ConversationStatus.Open && sender != MessageSender.Owner)
            Status = ConversationStatus.InProgress;

        return messageResult;
    }

    public Result Resolve()
    {
        if (Status == ConversationStatus.Closed)
            return Result.Error("CONVERSATION_CLOSED:Conversation is already closed");

        Status = ConversationStatus.Resolved;
        return Result.Success();
    }

    public Result Close()
    {
        Status = ConversationStatus.Closed;
        return Result.Success();
    }

    public Result Reopen()
    {
        if (Status == ConversationStatus.Open)
            return Result.Error("CONVERSATION_ALREADY_OPEN:Conversation is already open");

        Status = ConversationStatus.Open;
        return Result.Success();
    }

    public Result MarkEscalationSent()
    {
        EscalationSentAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkAsSpam()
    {
        IsSpam = true;
        return Result.Success();
    }

    public Result RestoreFromSpam()
    {
        IsSpam = false;
        return Result.Success();
    }

    /// <summary>
    /// Updates the conversation category. When <paramref name="resetUncertainty"/> is true,
    /// also clears the IsTriageUncertain flag (used for human-verified recategorization).
    /// </summary>
    public Result UpdateCategory(MessageCategory newCategory, bool resetUncertainty = false)
    {
        Category = newCategory;
        if (resetUncertainty)
            IsTriageUncertain = false;
        return Result.Success();
    }

    public Result AssignTo(Guid? userId, string? role)
    {
        AssignedToUserId = userId;
        AssignedToRole = role;
        return Result.Success();
    }

    public Result SetTriageResult(decimal confidence, bool isUncertain)
    {
        if (confidence is < 0 or > 1)
            return Result.Error("INVALID_CONFIDENCE:Triage confidence must be between 0 and 1");

        AiTriageConfidence = confidence;
        IsTriageUncertain = isUncertain;
        return Result.Success();
    }

    public ConversationDto ToDto() => new(
        Id,
        ClinicId,
        OwnerId,
        PatientId,
        Subject,
        Category,
        Status,
        Channel,
        _messages.Count,
        CreatedAt,
        LastMessageAt,
        IsSpam,
        AssignedToUserId,
        AssignedToRole,
        AiTriageConfidence,
        IsTriageUncertain);

    public ConversationWithMessagesDto ToDetailDto(
        bool includeInternalNotes = true,
        IReadOnlyList<string>? aiSuggestedReplies = null,
        PatientContextDto? patientContext = null) => new(
        Id,
        ClinicId,
        OwnerId,
        PatientId,
        Subject,
        Category,
        Status,
        Channel,
        AssignedToUserId,
        AssignedToRole,
        AiTriageConfidence,
        IsTriageUncertain,
        CreatedAt,
        LastMessageAt,
        _messages
            .Where(m => includeInternalNotes || !m.IsInternalNote)
            .Select(m => m.ToDto())
            .ToList()
            .AsReadOnly(),
        aiSuggestedReplies ?? Array.Empty<string>(),
        patientContext);
}
