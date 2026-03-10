using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.TransferConversation;

internal class TransferConversationValidator : AbstractValidator<TransferConversationCommand>
{
    public TransferConversationValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x)
            .Must(x => x.AssignedToUserId.HasValue || !string.IsNullOrWhiteSpace(x.AssignedToRole))
            .WithMessage("At least one of AssignedToUserId or AssignedToRole must be provided.");
    }
}
