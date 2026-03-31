using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.GenerateCheckInQr;

internal class GenerateCheckInQrValidator : AbstractValidator<GenerateCheckInQrCommand>
{
    public GenerateCheckInQrValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}
