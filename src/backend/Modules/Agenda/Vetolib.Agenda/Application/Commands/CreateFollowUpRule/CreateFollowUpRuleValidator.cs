using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CreateFollowUpRule;

internal class CreateFollowUpRuleValidator : AbstractValidator<CreateFollowUpRuleCommand>
{
    public CreateFollowUpRuleValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
        RuleFor(x => x.ConsultationType).NotEmpty().WithMessage("ConsultationType is required")
            .MaximumLength(100).WithMessage("ConsultationType must not exceed 100 characters");
        RuleFor(x => x.FollowUpDays).InclusiveBetween(1, 365)
            .WithMessage("FollowUpDays must be between 1 and 365");
        RuleFor(x => x.FollowUpReason).NotEmpty().WithMessage("FollowUpReason is required")
            .MaximumLength(200).WithMessage("FollowUpReason must not exceed 200 characters");
    }
}
