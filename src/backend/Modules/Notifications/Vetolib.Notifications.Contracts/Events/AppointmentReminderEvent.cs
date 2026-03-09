namespace Vetolib.Notifications.Contracts.Events;

/// <summary>
/// Published when an appointment reminder should be sent to the owner.
/// Maps to the appointment-reminder template.
/// </summary>
public record AppointmentReminderEvent
{
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string VetName { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
