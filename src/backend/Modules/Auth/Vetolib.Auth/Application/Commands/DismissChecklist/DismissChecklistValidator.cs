using FluentValidation;

namespace Vetolib.Auth.Application.Commands.DismissChecklist;

internal class DismissChecklistValidator : AbstractValidator<DismissChecklistCommand>
{
    public DismissChecklistValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
