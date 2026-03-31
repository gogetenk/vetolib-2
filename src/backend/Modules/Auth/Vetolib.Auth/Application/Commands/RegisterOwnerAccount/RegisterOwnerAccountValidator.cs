using FluentValidation;

namespace Vetolib.Auth.Application.Commands.RegisterOwnerAccount;

internal class RegisterOwnerAccountValidator : AbstractValidator<RegisterOwnerAccountCommand>
{
    public RegisterOwnerAccountValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
