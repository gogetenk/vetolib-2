using FluentValidation;

namespace Vetolib.AI.Application.Commands.DismissHealthAlert;

internal class DismissHealthAlertValidator : AbstractValidator<DismissHealthAlertCommand>
{
    public DismissHealthAlertValidator()
    {
        RuleFor(x => x.AlertId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.DismissedByName).NotEmpty();
    }
}
