using FluentValidation;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.CreateUser;

internal class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .ApplyPasswordPolicy();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.VetLicenseNumber)
            .NotEmpty()
            .When(x => x.Role == UserRole.Vet)
            .WithErrorCode("VET_LICENSE_REQUIRED")
            .WithMessage("A veterinary license number is required for the Vet role");
    }
}
