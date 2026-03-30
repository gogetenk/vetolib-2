namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Integration event published when an appointment reminder is due (24h before).
/// Consumed by the Notifications module to send the reminder email.
/// </summary>
public record AppointmentReminderDueIntegrationEvent
{
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string VetName { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
    public Guid ClinicId { get; init; }
    public string OwnerPhone { get; init; } = string.Empty;
}
