using FluentValidation;

namespace Vetolib.Auth.Application.Commands.RegisterClinic;

internal class RegisterClinicValidator : AbstractValidator<RegisterClinicCommand>
{
    public RegisterClinicValidator()
    {
        RuleFor(x => x.ClinicName)
            .NotEmpty()
            .WithMessage("ClinicName is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(10)
            .WithMessage("Password must be at least 10 characters")
            .Must(p => !string.IsNullOrEmpty(p) && p.Any(c => !char.IsLetterOrDigit(c)))
            .WithMessage("Password must contain at least one special character");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required");
    }
}
