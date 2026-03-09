using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreatePatient;

internal class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom de l'animal est requis");
        RuleFor(x => x.Breed).NotEmpty().WithMessage("La race est requise");
        RuleFor(x => x.OwnerName).NotEmpty().WithMessage("Le nom du proprietaire est requis");
        RuleFor(x => x.OwnerPhone).NotEmpty().WithMessage("Le telephone du proprietaire est requis");
        RuleFor(x => x.BirthDate).NotEqual(default(DateOnly)).WithMessage("La date de naissance est requise");
    }
}
