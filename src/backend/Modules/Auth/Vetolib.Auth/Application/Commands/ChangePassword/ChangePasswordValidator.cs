using FluentValidation;

namespace Vetolib.Auth.Application.Commands.ChangePassword;

internal class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Le mot de passe doit contenir au moins 10 caracteres")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre")
            .Matches("[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>/?`~]")
                .WithMessage("Le mot de passe doit contenir au moins un caractere special");
        RuleFor(x => x.UserId).NotEmpty();
    }
}
