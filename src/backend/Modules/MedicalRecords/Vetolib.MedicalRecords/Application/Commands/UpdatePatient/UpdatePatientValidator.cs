using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.UpdatePatient;

internal class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("PatientId est requis");
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom ne peut pas etre vide"));
        When(x => x.Breed is not null, () =>
            RuleFor(x => x.Breed).NotEmpty().WithMessage("La race ne peut pas etre vide"));
        When(x => x.OwnerPhone is not null, () =>
            RuleFor(x => x.OwnerPhone).NotEmpty().WithMessage("Le telephone ne peut pas etre vide"));
    }
}
