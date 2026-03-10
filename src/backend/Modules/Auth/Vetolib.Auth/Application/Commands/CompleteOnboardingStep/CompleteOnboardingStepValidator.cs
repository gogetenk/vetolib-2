using FluentValidation;

namespace Vetolib.Auth.Application.Commands.CompleteOnboardingStep;

internal class CompleteOnboardingStepValidator : AbstractValidator<CompleteOnboardingStepCommand>
{
    public CompleteOnboardingStepValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StepId).NotEmpty().WithMessage("StepId is required");
    }
}
