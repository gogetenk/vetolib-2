using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.UploadFiles;

internal class UploadFilesValidator : AbstractValidator<UploadFilesCommand>
{
    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx"];

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public UploadFilesValidator()
    {
        RuleFor(x => x.Files)
            .NotNull().WithMessage("Files collection is required.")
            .Must(f => f.Count > 0).WithMessage("At least one file is required.")
            .Must(f => f.Count <= 10).WithMessage("Maximum 10 files per upload.");

        RuleForEach(x => x.Files)
            .Must(f => f.Length > 0).WithMessage("File must not be empty.")
            .Must(f => f.Length <= MaxFileSizeBytes).WithMessage("File size must not exceed 10 MB.")
            .Must(f => AllowedExtensions.Contains(
                Path.GetExtension(f.FileName).ToLowerInvariant()))
            .WithMessage("File type is not allowed.");
    }
}
