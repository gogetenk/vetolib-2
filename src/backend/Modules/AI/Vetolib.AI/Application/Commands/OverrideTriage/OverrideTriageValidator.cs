using FluentValidation;

namespace Vetolib.AI.Application.Commands.OverrideTriage;

internal class OverrideTriageValidator : AbstractValidator<OverrideTriageCommand>
{
    public OverrideTriageValidator()
    {
        RuleFor(x => x.TriageId).NotEmpty();
        RuleFor(x => x.NewSeverity).IsInEnum();
    }
}
