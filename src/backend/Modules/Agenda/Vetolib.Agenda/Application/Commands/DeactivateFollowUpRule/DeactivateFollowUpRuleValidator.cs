using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.DeactivateFollowUpRule;

internal class DeactivateFollowUpRuleValidator : AbstractValidator<DeactivateFollowUpRuleCommand>
{
    public DeactivateFollowUpRuleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Follow-up rule Id is required.");
    }
}
