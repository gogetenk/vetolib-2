using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class MessageAttachment : BaseEntity
{
    public Guid MessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;

    private MessageAttachment() { } // EF Core

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png"];

    public static Result<MessageAttachment> Create(
        Guid messageId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath)
    {
        var errors = new List<ValidationError>();

        if (messageId == Guid.Empty)
            errors.Add(new ValidationError(nameof(messageId), "MessageId is required"));

        if (string.IsNullOrWhiteSpace(fileName))
            errors.Add(new ValidationError(nameof(fileName), "FileName is required"));

        if (string.IsNullOrWhiteSpace(contentType))
            errors.Add(new ValidationError(nameof(contentType), "ContentType is required"));
        else if (!AllowedContentTypes.Contains(contentType))
            errors.Add(new ValidationError(nameof(contentType), "Only JPEG and PNG images are allowed"));

        if (fileSizeBytes <= 0)
            errors.Add(new ValidationError(nameof(fileSizeBytes), "FileSizeBytes must be positive"));
        else if (fileSizeBytes > MaxFileSizeBytes)
            errors.Add(new ValidationError(nameof(fileSizeBytes), "File size cannot exceed 5 MB"));

        if (string.IsNullOrWhiteSpace(storagePath))
            errors.Add(new ValidationError(nameof(storagePath), "StoragePath is required"));

        if (errors.Count > 0)
            return Result<MessageAttachment>.Invalid(errors);

        return Result<MessageAttachment>.Success(new MessageAttachment
        {
            MessageId = messageId,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath
        });
    }

    public MessageAttachmentDto ToDto() => new(
        Id,
        MessageId,
        FileName,
        ContentType,
        FileSizeBytes,
        StoragePath);
}
