using FluentValidation;
using Vetolib.Notifications.Contracts.Enums;

namespace Vetolib.Notifications.Application.Commands.UpdateReminderConfig;

internal class UpdateReminderConfigValidator : AbstractValidator<UpdateReminderConfigCommand>
{
    public UpdateReminderConfigValidator()
    {
        RuleFor(x => x.Appointment24hLeadTimeHours)
            .InclusiveBetween(1, 72)
            .WithMessage("Appointment lead time must be between 1 and 72 hours.");

        RuleFor(x => x.VaccinationDueLeadTimeDays)
            .InclusiveBetween(1, 30)
            .WithMessage("Vaccination lead time must be between 1 and 30 days.");

        RuleFor(x => x.PreferredReminderChannel)
            .IsInEnum()
            .WithMessage("Invalid reminder channel. Must be Email, WhatsApp, Sms, Both, or All.");
    }
}
