using FluentValidation;
using Vetolib.MedicalRecords.Application.Services;

namespace Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;

internal class UploadPatientPhotoValidator : AbstractValidator<UploadPatientPhotoCommand>
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadPatientPhotoValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();

        RuleFor(x => x.PhotoData)
            .NotNull().WithMessage("Photo file is required.")
            .Must(data => data is { Length: > 0 }).WithMessage("Photo file must not be empty.")
            .Must(data => data is null || data.Length <= MaxFileSizeBytes)
            .WithMessage($"Photo file must not exceed 5 MB.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(ct => PhotoFileValidator.AllowedContentTypes.Contains(ct))
            .WithMessage("Content type must be image/jpeg, image/png, or image/webp.");

        // Magic-byte validation: file content must match declared content type
        RuleFor(x => x)
            .Must(cmd => cmd.PhotoData is null || cmd.PhotoData.Length == 0 || PhotoFileValidator.IsValid(cmd.ContentType, cmd.PhotoData))
            .WithMessage("File content does not match declared content type. The file may be corrupted or disguised.")
            .WithName("PhotoData");
    }
}
