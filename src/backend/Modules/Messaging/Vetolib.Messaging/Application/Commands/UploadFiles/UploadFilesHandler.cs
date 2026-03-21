using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.UploadFiles;

internal class UploadFilesHandler : IRequestHandler<UploadFilesCommand, Result<List<Guid>>>
{
    private readonly MessagingDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IClinicContext _clinicContext;

    public UploadFilesHandler(
        MessagingDbContext context,
        IFileStorage fileStorage,
        IClinicContext clinicContext)
    {
        _context = context;
        _fileStorage = fileStorage;
        _clinicContext = clinicContext;
    }

    public async Task<Result<List<Guid>>> Handle(UploadFilesCommand cmd, CancellationToken ct)
    {
        if (!FileTypeValidator.IsValidFileCount(cmd.Files.Count))
            return Result<List<Guid>>.Invalid(
                new ValidationError("Files", "Must upload between 1 and 5 files"));

        var pendingIds = new List<Guid>();

        foreach (var file in cmd.Files)
        {
            if (!FileTypeValidator.IsValidFileSize(file.Length))
                return Result<List<Guid>>.Invalid(
                    new ValidationError("Files", $"File '{file.FileName}' exceeds 10 MB limit or is empty"));

            // Read header bytes for magic byte validation
            var headerBuffer = new byte[4];
            using var headerStream = file.OpenReadStream();
            var bytesRead = await headerStream.ReadAsync(headerBuffer.AsMemory(0, 4), ct);
            headerStream.Position = 0; // Reset for upload

            if (bytesRead < 4)
                return Result<List<Guid>>.Invalid(
                    new ValidationError("Files", $"File '{file.FileName}' is too small to validate"));

            var detectedContentType = FileTypeValidator.DetectContentType(headerBuffer);
            if (detectedContentType is null)
                return Result<List<Guid>>.Invalid(
                    new ValidationError("Files", $"File '{file.FileName}' has unsupported type. Only PDF, JPG, and PNG are allowed"));

            // Upload to storage
            var storagePath = await _fileStorage.UploadAsync(headerStream, file.FileName, detectedContentType, ct);

            // Create PendingUpload entity
            var pendingResult = PendingUpload.Create(
                _clinicContext.ClinicId,
                file.FileName,
                detectedContentType,
                file.Length,
                storagePath);

            if (!pendingResult.IsSuccess)
                return Result<List<Guid>>.Invalid(pendingResult.ValidationErrors.ToList());

            _context.PendingUploads.Add(pendingResult.Value);
            pendingIds.Add(pendingResult.Value.Id);
        }

        await _context.SaveChangesAsync(ct);

        return Result<List<Guid>>.Success(pendingIds);
    }
}
