using FluentValidation;

namespace Vetolib.Auth.Application.Commands.RegisterWebhook;

internal class RegisterWebhookValidator : AbstractValidator<RegisterWebhookCommand>
{
    private static readonly HashSet<string> AllowedEventTypes = ["lab.result", "external.record"];

    public RegisterWebhookValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Secret)
            .NotEmpty()
            .MinimumLength(16)
            .MaximumLength(512);

        RuleFor(x => x.EventTypes)
            .NotEmpty()
            .WithMessage("At least one event type is required");

        RuleForEach(x => x.EventTypes)
            .Must(t => AllowedEventTypes.Contains(t))
            .WithMessage("Invalid event type. Allowed: lab.result, external.record");
    }
}
