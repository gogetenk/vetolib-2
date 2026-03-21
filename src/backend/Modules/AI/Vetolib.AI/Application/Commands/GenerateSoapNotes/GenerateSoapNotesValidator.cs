using FluentValidation;

namespace Vetolib.AI.Application.Commands.GenerateSoapNotes;

internal class GenerateSoapNotesValidator : AbstractValidator<GenerateSoapNotesCommand>
{
    public GenerateSoapNotesValidator()
    {
        RuleFor(x => x.Species)
            .NotEmpty()
            .WithMessage("Species is required.");

        RuleFor(x => x.Symptoms)
            .NotEmpty()
            .WithMessage("Symptoms are required.");

        RuleFor(x => x.PatientName)
            .NotEmpty()
            .WithMessage("Patient name is required.");
    }
}
