using FluentValidation;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.ChangeUserRole;

internal class ChangeUserRoleValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleValidator()
    {
        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("RequestingUserId is required");

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("TargetUserId is required");

        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage($"Role must be one of the following values: {string.Join(", ", Enum.GetNames<UserRole>())}");
    }
}
