using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.AcceptConsent;

internal class AcceptConsentValidator : AbstractValidator<AcceptConsentCommand>
{
    public AcceptConsentValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required");
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
        RuleFor(x => x.ConsentVersion).NotEmpty().WithMessage("ConsentVersion is required");
    }
}
