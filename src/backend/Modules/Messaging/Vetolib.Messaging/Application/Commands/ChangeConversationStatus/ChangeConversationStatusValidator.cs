using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.ChangeConversationStatus;

internal class ChangeConversationStatusValidator : AbstractValidator<ChangeConversationStatusCommand>
{
    public ChangeConversationStatusValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.Action)
            .IsInEnum().WithMessage("Action must be Resolve, Close, or Reopen.");
    }
}
