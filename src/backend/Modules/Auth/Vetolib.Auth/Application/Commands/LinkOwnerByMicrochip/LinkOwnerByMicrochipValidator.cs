using FluentValidation;

namespace Vetolib.Auth.Application.Commands.LinkOwnerByMicrochip;

internal class LinkOwnerByMicrochipValidator : AbstractValidator<LinkOwnerByMicrochipCommand>
{
    public LinkOwnerByMicrochipValidator()
    {
        RuleFor(x => x.OwnerAccountId).NotEmpty();
        RuleFor(x => x.MicrochipNumber).NotEmpty().Matches(@"^\d{15}$")
            .WithMessage("Microchip number must be 15 digits (ISO 11784/11785)");
    }
}
