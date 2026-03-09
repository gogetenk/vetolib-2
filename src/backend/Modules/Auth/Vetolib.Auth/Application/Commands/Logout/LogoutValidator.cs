using FluentValidation;

namespace Vetolib.Auth.Application.Commands.Logout;

internal class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId est requis");
    }
}
