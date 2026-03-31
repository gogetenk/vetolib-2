using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.SubmitVisitFeedback;

internal class SubmitVisitFeedbackValidator : AbstractValidator<SubmitVisitFeedbackCommand>
{
    public SubmitVisitFeedbackValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
        RuleFor(x => x.Comment).MaximumLength(1000)
            .WithMessage("Comment must not exceed 1000 characters");
    }
}
