using FluentValidation;

namespace Vetolib.Auth.Application.Commands.RefreshToken;

internal class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Le refresh token est requis");
    }
}
