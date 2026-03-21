using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.SendWhatsAppTest;

internal sealed class SendWhatsAppTestValidator : AbstractValidator<SendWhatsAppTestCommand>
{
    public SendWhatsAppTestValidator()
    {
        RuleFor(x => x.RecipientPhone)
            .NotEmpty().WithMessage("Recipient phone is required")
            .Matches(@"^\+\d{7,15}$").WithMessage("Phone must be in E.164 format");

        RuleFor(x => x.TemplateName)
            .NotEmpty().WithMessage("Template name is required");
    }
}
