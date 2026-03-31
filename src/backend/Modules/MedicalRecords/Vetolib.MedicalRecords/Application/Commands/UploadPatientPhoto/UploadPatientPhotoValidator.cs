using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;

internal class UploadPatientPhotoValidator : AbstractValidator<UploadPatientPhotoCommand>
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

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
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Content type must be image/jpeg, image/png, or image/webp.");
    }
}
