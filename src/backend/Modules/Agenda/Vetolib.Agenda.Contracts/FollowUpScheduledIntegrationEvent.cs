namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Integration event published when a follow-up appointment is automatically scheduled.
/// Consumed by the Notifications module to notify the owner.
/// </summary>
public record FollowUpScheduledIntegrationEvent
{
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string VetName { get; init; } = string.Empty;
    public DateTime FollowUpDate { get; init; }
    public string FollowUpReason { get; init; } = string.Empty;
    public Guid ClinicId { get; init; }
}
