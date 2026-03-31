using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.MarkWaitingRoom;

internal class MarkWaitingRoomValidator : AbstractValidator<MarkWaitingRoomCommand>
{
    public MarkWaitingRoomValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("AppointmentId is required");
    }
}
