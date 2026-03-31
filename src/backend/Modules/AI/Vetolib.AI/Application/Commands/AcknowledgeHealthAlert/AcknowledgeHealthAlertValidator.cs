using FluentValidation;

namespace Vetolib.AI.Application.Commands.AcknowledgeHealthAlert;

internal class AcknowledgeHealthAlertValidator : AbstractValidator<AcknowledgeHealthAlertCommand>
{
    public AcknowledgeHealthAlertValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("AlertId is required.");
    }
}
