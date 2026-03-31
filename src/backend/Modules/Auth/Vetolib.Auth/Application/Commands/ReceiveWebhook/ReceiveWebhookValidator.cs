using FluentValidation;

namespace Vetolib.Auth.Application.Commands.ReceiveWebhook;

internal class ReceiveWebhookValidator : AbstractValidator<ReceiveWebhookCommand>
{
    public ReceiveWebhookValidator()
    {
        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature is required.");

        RuleFor(x => x.RawBody)
            .NotEmpty().WithMessage("RawBody is required.");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("EventType is required.");
    }
}
