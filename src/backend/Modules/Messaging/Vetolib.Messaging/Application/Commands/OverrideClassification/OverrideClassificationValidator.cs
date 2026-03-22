using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.OverrideClassification;

internal class OverrideClassificationValidator : AbstractValidator<OverrideClassificationCommand>
{
    public OverrideClassificationValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.Urgency).IsInEnum();
        RuleFor(x => x.Category).IsInEnum();
    }
}
