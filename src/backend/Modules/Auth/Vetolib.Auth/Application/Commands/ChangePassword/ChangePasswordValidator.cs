using FluentValidation;

namespace Vetolib.Auth.Application.Commands.ChangePassword;

internal class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .ApplyPasswordPolicy();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
