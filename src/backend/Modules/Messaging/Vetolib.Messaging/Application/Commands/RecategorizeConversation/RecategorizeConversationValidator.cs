using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.RecategorizeConversation;

internal class RecategorizeConversationValidator : AbstractValidator<RecategorizeConversationCommand>
{
    public RecategorizeConversationValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.NewCategory)
            .IsInEnum().WithMessage("NewCategory must be a valid MessageCategory.");
    }
}
