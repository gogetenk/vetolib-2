using FluentValidation;

namespace Vetolib.Auth.Application.Commands.DeactivateUser;

internal class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("RequestingUserId est requis");

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("TargetUserId est requis");
    }
}
