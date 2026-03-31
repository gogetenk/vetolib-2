using FluentValidation;

namespace Vetolib.Auth.Application.Commands.GetOrCreateReferralCode;

internal class GetOrCreateReferralCodeValidator : AbstractValidator<GetOrCreateReferralCodeCommand>
{
    public GetOrCreateReferralCodeValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}
