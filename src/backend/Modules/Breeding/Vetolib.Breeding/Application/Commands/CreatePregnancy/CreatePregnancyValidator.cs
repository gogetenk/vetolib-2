using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.CreatePregnancy;

internal class CreatePregnancyValidator : AbstractValidator<CreatePregnancyCommand>
{
    public CreatePregnancyValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.MatingDate).NotEmpty();
        RuleFor(x => x.MatingMethod).IsInEnum();
    }
}
