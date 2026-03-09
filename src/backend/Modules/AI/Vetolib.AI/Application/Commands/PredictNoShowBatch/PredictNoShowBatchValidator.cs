using FluentValidation;

namespace Vetolib.AI.Application.Commands.PredictNoShowBatch;

internal class PredictNoShowBatchValidator : AbstractValidator<PredictNoShowBatchCommand>
{
    public PredictNoShowBatchValidator()
    {
        RuleFor(x => x.Date)
            .NotEqual(DateOnly.MinValue)
            .WithMessage("Date must not be the default value.");
    }
}
