using FluentValidation;

namespace Vetolib.Auth.Application.Commands.InviteUser;

internal class InviteUserValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
