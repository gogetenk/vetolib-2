using Ardalis.Result;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Domain;

internal class ReminderConfig : BaseEntity, IMultiTenant
{
    public bool Appointment24hEnabled { get; private set; } = true;
    public bool VaccinationDueEnabled { get; private set; } = true;
    public bool FollowUpEnabled { get; private set; } = true;
    public int Appointment24hLeadTimeHours { get; private set; } = 24;
    public int VaccinationDueLeadTimeDays { get; private set; } = 7;
    public ReminderChannel PreferredReminderChannel { get; private set; } = ReminderChannel.Email;
    public Guid ClinicId { get; private set; }

    private ReminderConfig() { }

    public static Result<ReminderConfig> CreateDefault(Guid clinicId)
    {
        if (clinicId == Guid.Empty)
            return Result<ReminderConfig>.Invalid(new ValidationError("ClinicId is required."));

        return Result<ReminderConfig>.Success(new ReminderConfig { ClinicId = clinicId });
    }

    public Result Update(
        bool appointment24hEnabled,
        bool vaccinationDueEnabled,
        bool followUpEnabled,
        int appointment24hLeadTimeHours,
        int vaccinationDueLeadTimeDays,
        ReminderChannel preferredReminderChannel)
    {
        if (appointment24hLeadTimeHours < 1 || appointment24hLeadTimeHours > 72)
            return Result.Invalid(new ValidationError("Appointment lead time must be between 1 and 72 hours."));

        if (vaccinationDueLeadTimeDays < 1 || vaccinationDueLeadTimeDays > 30)
            return Result.Invalid(new ValidationError("Vaccination lead time must be between 1 and 30 days."));

        if (!Enum.IsDefined(preferredReminderChannel))
            return Result.Invalid(new ValidationError("Invalid reminder channel."));

        Appointment24hEnabled = appointment24hEnabled;
        VaccinationDueEnabled = vaccinationDueEnabled;
        FollowUpEnabled = followUpEnabled;
        Appointment24hLeadTimeHours = appointment24hLeadTimeHours;
        VaccinationDueLeadTimeDays = vaccinationDueLeadTimeDays;
        PreferredReminderChannel = preferredReminderChannel;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
