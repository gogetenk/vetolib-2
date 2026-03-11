using System.Text.Json.Serialization;

namespace Vetolib.Agenda.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppointmentStatus
{
    Scheduled,
    CheckedIn,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}
