using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.ClassificationFeedback;

internal class ClassificationFeedbackValidator : AbstractValidator<ClassificationFeedbackCommand>
{
    public ClassificationFeedbackValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.MessageId).NotEmpty();
    }
}
