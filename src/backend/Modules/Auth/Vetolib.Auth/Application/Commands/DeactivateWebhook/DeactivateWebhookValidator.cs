using FluentValidation;

namespace Vetolib.Auth.Application.Commands.DeactivateWebhook;

internal class DeactivateWebhookValidator : AbstractValidator<DeactivateWebhookCommand>
{
    public DeactivateWebhookValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
