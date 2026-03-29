using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.CreateLitter;

internal class CreateLitterValidator : AbstractValidator<CreateLitterCommand>
{
    public CreateLitterValidator()
    {
        RuleFor(x => x.MotherPatientId).NotEmpty();
        RuleFor(x => x.BornCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AliveCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BirthDate).NotEmpty();
    }
}
