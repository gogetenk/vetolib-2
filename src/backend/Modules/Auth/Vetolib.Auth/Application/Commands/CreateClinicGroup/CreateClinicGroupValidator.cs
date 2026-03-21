using FluentValidation;

namespace Vetolib.Auth.Application.Commands.CreateClinicGroup;

internal class CreateClinicGroupValidator : AbstractValidator<CreateClinicGroupCommand>
{
    public CreateClinicGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(256).WithMessage("Group name must not exceed 256 characters");

        RuleFor(x => x.OwnerUserId)
            .NotEqual(Guid.Empty).WithMessage("Owner user ID is required");
    }
}
