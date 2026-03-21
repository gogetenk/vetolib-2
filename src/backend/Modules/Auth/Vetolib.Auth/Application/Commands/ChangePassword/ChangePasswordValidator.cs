using FluentValidation;

namespace Vetolib.Auth.Application.Commands.ChangePassword;

internal class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Password must contain at least 10 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>/?`~]")
                .WithMessage("Password must contain at least one special character");
        RuleFor(x => x.UserId).NotEmpty();
    }
}
