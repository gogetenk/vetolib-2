namespace Vetolib.Messaging.Contracts;

public record MessageAttachmentDto(
    Guid Id,
    Guid MessageId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string Url
);
