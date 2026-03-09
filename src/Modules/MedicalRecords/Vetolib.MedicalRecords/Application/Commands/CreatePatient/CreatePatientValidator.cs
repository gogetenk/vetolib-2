using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreatePatient;

internal class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom de l'animal est requis");
        RuleFor(x => x.Species).NotEmpty().WithMessage("L'espece est requise");
        RuleFor(x => x.Breed).NotEmpty().WithMessage("La race est requise");
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("Le proprietaire est requis");
    }
}
