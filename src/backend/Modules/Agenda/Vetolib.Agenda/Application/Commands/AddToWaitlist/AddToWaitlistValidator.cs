using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.AddToWaitlist;

internal class AddToWaitlistValidator : AbstractValidator<AddToWaitlistCommand>
{
    public AddToWaitlistValidator()
    {
        RuleFor(x => x.ClinicId)
            .NotEmpty()
            .WithMessage("ClinicId is required");

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("PatientId is required");

        RuleFor(x => x.OwnerName)
            .NotEmpty()
            .WithMessage("Owner name is required")
            .MaximumLength(256);

        RuleFor(x => x.OwnerPhone)
            .NotEmpty()
            .WithMessage("Owner phone is required")
            .MaximumLength(50);

        RuleFor(x => x.OwnerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.OwnerEmail))
            .WithMessage("Owner email must be a valid email address")
            .MaximumLength(256);

        RuleFor(x => x.PreferredDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Preferred date must be today or in the future");

        RuleFor(x => x.PreferredTimeSlot)
            .IsInEnum()
            .WithMessage("Preferred time slot must be a valid value");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => x.Reason is not null);
    }
}
