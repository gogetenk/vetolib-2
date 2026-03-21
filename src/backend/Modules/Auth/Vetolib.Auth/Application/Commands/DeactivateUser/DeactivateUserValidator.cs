using FluentValidation;

namespace Vetolib.Auth.Application.Commands.DeactivateUser;

internal class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("RequestingUserId is required");

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("TargetUserId is required");
    }
}
