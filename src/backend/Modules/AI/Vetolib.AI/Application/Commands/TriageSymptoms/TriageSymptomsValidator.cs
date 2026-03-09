using FluentValidation;

namespace Vetolib.AI.Application.Commands.TriageSymptoms;

internal class TriageSymptomsValidator : AbstractValidator<TriageSymptomsCommand>
{
    public TriageSymptomsValidator()
    {
        RuleFor(x => x.Symptoms)
            .NotEmpty()
            .WithMessage("Symptoms are required.");

        RuleFor(x => x.Species)
            .NotEmpty()
            .WithMessage("Species is required.");
    }
}
