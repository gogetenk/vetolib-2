using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.SetLineage;

internal class SetLineageValidator : AbstractValidator<SetLineageCommand>
{
    public SetLineageValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.RegistryNumber)
            .MaximumLength(100)
            .When(x => x.RegistryNumber is not null);
    }
}
