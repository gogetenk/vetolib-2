using FluentValidation;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.UpdatePreference;

internal class UpdatePreferenceValidator : AbstractValidator<UpdatePreferenceCommand>
{
    public UpdatePreferenceValidator()
    {
        RuleFor(x => x.Key)
            .IsInEnum().WithMessage("Key must be a valid PreferenceKey");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required");

        RuleFor(x => x)
            .Must(cmd => !(cmd.Key == PreferenceKey.AIDrugInteractions &&
                           cmd.Value.Equals("false", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Drug interaction alerts cannot be disabled")
            .WithName("Key");
    }
}
