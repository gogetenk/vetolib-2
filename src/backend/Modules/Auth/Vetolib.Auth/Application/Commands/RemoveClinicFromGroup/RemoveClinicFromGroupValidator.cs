using FluentValidation;

namespace Vetolib.Auth.Application.Commands.RemoveClinicFromGroup;

internal class RemoveClinicFromGroupValidator : AbstractValidator<RemoveClinicFromGroupCommand>
{
    public RemoveClinicFromGroupValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("GroupId is required");

        RuleFor(x => x.ClinicId)
            .NotEmpty()
            .WithMessage("ClinicId is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("RequestingUserId is required");
    }
}
