using FluentValidation;

namespace Vetolib.Auth.Application.Commands.AddClinicToGroup;

internal class AddClinicToGroupValidator : AbstractValidator<AddClinicToGroupCommand>
{
    public AddClinicToGroupValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("GroupId is required");

        RuleFor(x => x.ClinicId)
            .NotEmpty()
            .WithMessage("ClinicId is required");
    }
}
