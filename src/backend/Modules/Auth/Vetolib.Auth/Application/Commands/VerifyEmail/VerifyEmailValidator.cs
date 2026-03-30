using FluentValidation;

namespace Vetolib.Auth.Application.Commands.VerifyEmail;

internal class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Verification token is required");
    }
}
