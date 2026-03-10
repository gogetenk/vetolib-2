using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.CreateOutboundConversation;

internal class CreateOutboundConversationValidator : AbstractValidator<CreateOutboundConversationCommand>
{
    public CreateOutboundConversationValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters.");

        RuleFor(x => x.InitialMessageBody)
            .NotEmpty().WithMessage("Initial message body is required.")
            .MaximumLength(2000).WithMessage("Initial message body cannot exceed 2000 characters.");
    }
}
