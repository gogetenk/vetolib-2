using FluentValidation;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.CreateUser;

internal class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty()
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caracteres")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre");
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.VetLicenseNumber)
            .NotEmpty()
            .When(x => x.Role == UserRole.Vet)
            .WithErrorCode("VET_LICENSE_REQUIRED")
            .WithMessage("Un numero de licence veterinaire est requis pour le role Vet");
    }
}
