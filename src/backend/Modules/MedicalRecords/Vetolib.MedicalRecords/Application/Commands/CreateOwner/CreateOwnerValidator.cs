using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreateOwner;

internal class CreateOwnerValidator : AbstractValidator<CreateOwnerCommand>
{
    public CreateOwnerValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required");
    }
}
