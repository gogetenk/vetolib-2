using FluentValidation;

namespace Vetolib.Auth.Application;

/// <summary>
/// Centralized password validation policy applied consistently across all Auth validators.
/// Policy: min 8 chars, at least 1 uppercase, 1 lowercase, 1 digit, 1 special character.
/// </summary>
internal static class PasswordRules
{
    public const int MinimumLength = 8;

    /// <summary>
    /// Applies the standard password policy rules to a string property.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyPasswordPolicy<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(MinimumLength)
            .WithMessage($"Password must be at least {MinimumLength} characters")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?`~]")
            .WithMessage("Password must contain at least one special character");
    }
}
