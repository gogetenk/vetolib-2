using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.UpdatePatient;

internal class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("PatientId is required");
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be empty"));
        When(x => x.Breed is not null, () =>
            RuleFor(x => x.Breed).NotEmpty().WithMessage("Breed cannot be empty"));
        When(x => x.OwnerPhone is not null, () =>
            RuleFor(x => x.OwnerPhone).NotEmpty().WithMessage("Phone number cannot be empty"));
        When(x => x.MicrochipNumber is not null, () =>
            RuleFor(x => x.MicrochipNumber).Matches(@"^\d{15}$")
                .WithMessage("Microchip number must be 15 digits (ISO 11784/11785)"));
    }
}
