using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.AddMessageToRecord;

internal class AddMessageToRecordValidator : AbstractValidator<AddMessageToRecordCommand>
{
    public AddMessageToRecordValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required");

        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("MessageId is required");
    }
}
