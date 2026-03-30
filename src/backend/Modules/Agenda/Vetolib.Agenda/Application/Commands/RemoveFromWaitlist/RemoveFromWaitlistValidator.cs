using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.RemoveFromWaitlist;

internal class RemoveFromWaitlistValidator : AbstractValidator<RemoveFromWaitlistCommand>
{
    public RemoveFromWaitlistValidator()
    {
        RuleFor(x => x.EntryId)
            .NotEmpty()
            .WithMessage("EntryId is required");
    }
}
