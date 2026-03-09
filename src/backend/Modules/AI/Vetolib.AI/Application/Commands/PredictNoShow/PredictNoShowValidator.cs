using FluentValidation;

namespace Vetolib.AI.Application.Commands.PredictNoShow;

internal class PredictNoShowValidator : AbstractValidator<PredictNoShowCommand>
{
    public PredictNoShowValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}
