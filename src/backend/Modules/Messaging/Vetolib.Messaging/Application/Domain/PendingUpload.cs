using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class PendingUpload : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private PendingUpload() { } // EF Core

    public static Result<PendingUpload> Create(
        Guid clinicId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(fileName))
            errors.Add(new ValidationError(nameof(fileName), "FileName is required"));

        if (string.IsNullOrWhiteSpace(contentType))
            errors.Add(new ValidationError(nameof(contentType), "ContentType is required"));

        if (fileSizeBytes <= 0)
            errors.Add(new ValidationError(nameof(fileSizeBytes), "FileSizeBytes must be positive"));

        if (string.IsNullOrWhiteSpace(storagePath))
            errors.Add(new ValidationError(nameof(storagePath), "StoragePath is required"));

        if (errors.Count > 0)
            return Result<PendingUpload>.Invalid(errors);

        var now = DateTime.UtcNow;
        return Result<PendingUpload>.Success(new PendingUpload
        {
            ClinicId = clinicId,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath,
            UploadedAt = now,
            ExpiresAt = now.AddHours(24)
        });
    }
}
