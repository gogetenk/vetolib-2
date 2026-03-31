using FluentValidation;

namespace Vetolib.Auth.Application.Commands.InviteVet;

internal class InviteVetValidator : AbstractValidator<InviteVetCommand>
{
    public InviteVetValidator()
    {
        RuleFor(x => x.VetEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.OwnerName).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.PetName).NotEmpty().MinimumLength(1).MaximumLength(200);
        RuleFor(x => x.Message).MaximumLength(1000);
    }
}
