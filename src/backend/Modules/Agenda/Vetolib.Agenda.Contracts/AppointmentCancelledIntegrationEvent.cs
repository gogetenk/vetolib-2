namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Integration event published when an appointment is cancelled.
/// Consumed by the Notifications module to send a cancellation email to the owner.
/// </summary>
public record AppointmentCancelledIntegrationEvent
{
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string VetName { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string ClinicName { get; init; } = string.Empty;
    public string? CancellationReason { get; init; }
    public string PreferredLanguage { get; init; } = "en";
}
