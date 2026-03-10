using FluentValidation;

namespace Vetolib.Preferences.Application.Commands.RevokeConsent;

internal class RevokeConsentValidator : AbstractValidator<RevokeConsentCommand>
{
    public RevokeConsentValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid PreferenceCategory");
    }
}
