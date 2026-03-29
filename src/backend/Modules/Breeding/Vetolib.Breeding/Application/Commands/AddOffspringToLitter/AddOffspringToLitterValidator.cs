using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.AddOffspringToLitter;

internal class AddOffspringToLitterValidator : AbstractValidator<AddOffspringToLitterCommand>
{
    public AddOffspringToLitterValidator()
    {
        RuleFor(x => x.LitterId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
