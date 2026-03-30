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
        When(x => x.MicrochipNumber is not null, () =>
            RuleFor(x => x.MicrochipNumber).Matches(@"^\d{15}$")
                .WithMessage("Microchip number must be 15 digits (ISO 11784/11785)"));
        When(x => x.OwnerEmail is not null, () =>
            RuleFor(x => x.OwnerEmail).EmailAddress()
                .WithMessage("A valid email address is required when provided"));
    }
}
