using FluentValidation;

namespace Vetolib.Auth.Application.Commands.DismissWelcomeBanner;

internal class DismissWelcomeBannerValidator : AbstractValidator<DismissWelcomeBannerCommand>
{
    public DismissWelcomeBannerValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
