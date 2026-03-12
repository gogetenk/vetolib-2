using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.UpdateConsultationType;

internal class UpdateConsultationTypeValidator : AbstractValidator<UpdateConsultationTypeCommand>
{
    public UpdateConsultationTypeValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(10, 180)
            .WithMessage("Duration must be between 10 and 180 minutes");
    }
}
