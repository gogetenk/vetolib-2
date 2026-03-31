using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.DeleteStaffSchedule;

internal class DeleteStaffScheduleValidator : AbstractValidator<DeleteStaffScheduleCommand>
{
    public DeleteStaffScheduleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff schedule Id is required.");
    }
}
