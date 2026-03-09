using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreateOwner;

internal class CreateOwnerValidator : AbstractValidator<CreateOwnerCommand>
{
    public CreateOwnerValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Le prenom est requis");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Le nom est requis");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Un email valide est requis");
    }
}
