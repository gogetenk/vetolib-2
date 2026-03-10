using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.CreateOwnerConversation;

internal class CreateOwnerConversationValidator : AbstractValidator<CreateOwnerConversationCommand>
{
    public CreateOwnerConversationValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required");
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
        RuleFor(x => x.Subject).NotEmpty().WithMessage("Subject is required").MaximumLength(200);
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required")
            .MaximumLength(2000).WithMessage("Message body cannot exceed 2000 characters");
    }
}
