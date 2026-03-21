using System.Text.Json.Serialization;

namespace Vetolib.Notifications.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReminderType
{
    Appointment24h,
    VaccinationDue,
    FollowUp
}
