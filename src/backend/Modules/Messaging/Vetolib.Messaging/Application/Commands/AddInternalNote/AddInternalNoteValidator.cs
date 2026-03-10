using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.AddInternalNote;

internal class AddInternalNoteValidator : AbstractValidator<AddInternalNoteCommand>
{
    public AddInternalNoteValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Note body is required.")
            .MaximumLength(2000).WithMessage("Note body cannot exceed 2000 characters.");
    }
}
