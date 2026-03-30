namespace Vetolib.Notifications.Contracts.Events;

/// <summary>
/// Published when a WhatsApp reminder should be sent.
/// Consumed by the Messaging module which handles actual WhatsApp delivery.
/// </summary>
public record SendWhatsAppReminderEvent
{
    public string OwnerPhone { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string VetName { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
