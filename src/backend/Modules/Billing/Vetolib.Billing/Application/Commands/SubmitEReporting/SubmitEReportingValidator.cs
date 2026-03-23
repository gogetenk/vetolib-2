using FluentValidation;

namespace Vetolib.Billing.Application.Commands.SubmitEReporting;

internal class SubmitEReportingValidator : AbstractValidator<SubmitEReportingCommand>
{
    public SubmitEReportingValidator()
    {
        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("PeriodStart is required");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("PeriodEnd is required")
            .GreaterThanOrEqualTo(x => x.PeriodStart)
            .WithMessage("PeriodEnd must be >= PeriodStart");
    }
}
