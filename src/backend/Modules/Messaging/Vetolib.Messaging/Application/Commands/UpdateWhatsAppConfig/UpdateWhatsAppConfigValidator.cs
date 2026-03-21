using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.UpdateWhatsAppConfig;

internal sealed class UpdateWhatsAppConfigValidator : AbstractValidator<UpdateWhatsAppConfigCommand>
{
    public UpdateWhatsAppConfigValidator()
    {
        RuleFor(x => x.WabaId)
            .NotEmpty().WithMessage("WABA ID is required");

        RuleFor(x => x.PhoneNumberId)
            .NotEmpty().WithMessage("Phone Number ID is required");

        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access Token is required");
    }
}
