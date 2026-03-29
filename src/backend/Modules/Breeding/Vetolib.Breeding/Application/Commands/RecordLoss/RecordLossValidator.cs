using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.RecordLoss;

internal class RecordLossValidator : AbstractValidator<RecordLossCommand>
{
    public RecordLossValidator()
    {
        RuleFor(x => x.PregnancyId).NotEmpty();
        RuleFor(x => x.LossDate).NotEmpty();
        RuleFor(x => x.Outcome).IsInEnum();
    }
}
