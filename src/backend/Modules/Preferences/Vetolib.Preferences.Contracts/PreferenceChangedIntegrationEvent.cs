using MassTransit;

namespace Vetolib.Preferences.Contracts;

/// <summary>
/// Published via MassTransit when a preference value changes.
/// Consumers (e.g. Notifications module) can react to opt-in/opt-out changes.
/// </summary>
public record PreferenceChangedIntegrationEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; } = NewId.NextGuid();
    public Guid ClinicId { get; init; }
    public Guid UserId { get; init; }
    public PreferenceKey Key { get; init; }
    public PreferenceCategory Category { get; init; }
    public string? PreviousValue { get; init; }
    public string NewValue { get; init; } = string.Empty;
    public PreferenceSource Source { get; init; }
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
}
