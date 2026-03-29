using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.CompleteCheck;

internal class CompleteCheckValidator : AbstractValidator<CompleteCheckCommand>
{
    public CompleteCheckValidator()
    {
        RuleFor(x => x.CheckId).NotEmpty();
    }
}
