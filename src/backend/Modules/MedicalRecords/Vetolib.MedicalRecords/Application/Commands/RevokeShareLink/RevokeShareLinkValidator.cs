using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.RevokeShareLink;

internal class RevokeShareLinkValidator : AbstractValidator<RevokeShareLinkCommand>
{
    public RevokeShareLinkValidator()
    {
        RuleFor(x => x.LinkId)
            .NotEmpty().WithMessage("LinkId is required.");

        RuleFor(x => x.OwnerAccountId)
            .NotEmpty().WithMessage("OwnerAccountId is required.");
    }
}
