using FluentValidation;

namespace Vetolib.Auth.Application.Commands.SwitchClinic;

internal class SwitchClinicValidator : AbstractValidator<SwitchClinicCommand>
{
    public SwitchClinicValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.TargetClinicId)
            .NotEmpty()
            .WithMessage("TargetClinicId is required");
    }
}
