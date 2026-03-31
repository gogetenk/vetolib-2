using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CreateStaffSchedule;

internal class CreateStaffScheduleValidator : AbstractValidator<CreateStaffScheduleCommand>
{
    public CreateStaffScheduleValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time");
        RuleFor(x => x.ShiftType).IsInEnum();
    }
}
