using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.DeletePatientPhoto;

internal class DeletePatientPhotoValidator : AbstractValidator<DeletePatientPhotoCommand>
{
    public DeletePatientPhotoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("PatientId is required.");
    }
}
