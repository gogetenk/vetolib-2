using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.ConvertToAppointment;

internal class ConvertToAppointmentValidator : AbstractValidator<ConvertToAppointmentCommand>
{
    public ConvertToAppointmentValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => x.Notes is not null);
    }
}
