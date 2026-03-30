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
            .ApplyPasswordPolicy();

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required");
    }
}
