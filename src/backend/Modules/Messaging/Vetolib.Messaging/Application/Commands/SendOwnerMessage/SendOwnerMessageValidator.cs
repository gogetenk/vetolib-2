using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.SendOwnerMessage;

internal class SendOwnerMessageValidator : AbstractValidator<SendOwnerMessageCommand>
{
    public SendOwnerMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("ConversationId is required");
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required");
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required")
            .MaximumLength(2000).WithMessage("Message body cannot exceed 2000 characters");
    }
}
