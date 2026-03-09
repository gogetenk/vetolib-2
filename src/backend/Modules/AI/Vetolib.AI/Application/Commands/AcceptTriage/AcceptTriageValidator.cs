using FluentValidation;

namespace Vetolib.AI.Application.Commands.AcceptTriage;

internal class AcceptTriageValidator : AbstractValidator<AcceptTriageCommand>
{
    public AcceptTriageValidator()
    {
        RuleFor(x => x.TriageId).NotEmpty();
    }
}
