using FluentValidation;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.ChangeUserRole;

internal class ChangeUserRoleValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleValidator()
    {
        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("RequestingUserId est requis");

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("TargetUserId est requis");

        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage($"Le role doit etre l'une des valeurs : {string.Join(", ", Enum.GetNames<UserRole>())}");
    }
}
