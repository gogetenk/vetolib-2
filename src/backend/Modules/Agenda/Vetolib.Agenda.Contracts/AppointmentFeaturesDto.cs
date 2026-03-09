namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Aggregated features for an appointment used by the AI no-show prediction model.
/// Contains only behavioral/scheduling data — no PII identifiers.
/// </summary>
public record AppointmentFeaturesDto(
    Guid AppointmentId,
    Guid AnimalId,
    DateOnly Date,
    TimeOnly StartTime,
    string? ConsultationType,
    bool WasReminderSent,
    /// <summary>Total appointments for this animal/owner.</summary>
    int OwnerTotalAppointments,
    /// <summary>Number of no-shows in owner history.</summary>
    int OwnerNoShowCount,
    /// <summary>Days since the last visit (0 if first visit).</summary>
    int DaysSinceLastVisit,
    /// <summary>Days between appointment creation and appointment date (lead time).</summary>
    int LeadTimeDays);
