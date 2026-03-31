using FluentValidation;

namespace Vetolib.Auth.Application.Commands.OwnerPortalLogin;

internal class OwnerPortalLoginValidator : AbstractValidator<OwnerPortalLoginCommand>
{
    public OwnerPortalLoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
