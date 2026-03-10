using FluentValidation;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.BulkUpdatePreferences;

internal class BulkUpdatePreferencesValidator : AbstractValidator<BulkUpdatePreferencesCommand>
{
    public BulkUpdatePreferencesValidator()
    {
        RuleFor(x => x.Preferences)
            .NotEmpty().WithMessage("Preferences list cannot be empty")
            .Must(p => p.Count <= 50).WithMessage("Cannot update more than 50 preferences at once");

        RuleForEach(x => x.Preferences)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.Key)
                    .IsInEnum().WithMessage("Key must be a valid PreferenceKey");

                item.RuleFor(x => x.Value)
                    .NotEmpty().WithMessage("Value is required");

                item.RuleFor(x => x)
                    .Must(i => !(i.Key == PreferenceKey.AIDrugInteractions &&
                                 i.Value.Equals("false", StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("Drug interaction alerts cannot be disabled")
                    .WithName("Key");
            });
    }
}
