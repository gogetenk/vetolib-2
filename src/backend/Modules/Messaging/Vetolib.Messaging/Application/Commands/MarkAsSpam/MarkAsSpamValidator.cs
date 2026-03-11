using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.MarkAsSpam;

internal class MarkAsSpamValidator : AbstractValidator<MarkAsSpamCommand>
{
    public MarkAsSpamValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");
    }
}
