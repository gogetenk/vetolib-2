using Vetolib.Notifications.Contracts.Enums;

namespace Vetolib.Notifications.Contracts.Dtos;

public record ReminderConfigDto(
    bool Appointment24hEnabled,
    bool VaccinationDueEnabled,
    bool FollowUpEnabled,
    int Appointment24hLeadTimeHours,
    int VaccinationDueLeadTimeDays);
