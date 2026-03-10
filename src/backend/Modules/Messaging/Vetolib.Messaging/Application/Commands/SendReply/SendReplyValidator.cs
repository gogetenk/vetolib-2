using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.SendReply;

internal class SendReplyValidator : AbstractValidator<SendReplyCommand>
{
    public SendReplyValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Reply body is required.")
            .MaximumLength(2000).WithMessage("Reply body cannot exceed 2000 characters.");
    }
}
