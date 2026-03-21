using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreatePatient;

internal class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Animal name is required");
        RuleFor(x => x.Breed).NotEmpty().WithMessage("Breed is required");
        RuleFor(x => x.OwnerName).NotEmpty().WithMessage("Owner name is required");
        RuleFor(x => x.OwnerPhone).NotEmpty().WithMessage("Owner phone number is required");
        RuleFor(x => x.BirthDate).NotEqual(default(DateOnly)).WithMessage("Birth date is required");
    }
}
