using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CheckInFromQr;

internal class CheckInFromQrValidator : AbstractValidator<CheckInFromQrCommand>
{
    public CheckInFromQrValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.PatientName).NotEmpty();
        RuleFor(x => x.OwnerName).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Signature).NotEmpty();
    }
}
