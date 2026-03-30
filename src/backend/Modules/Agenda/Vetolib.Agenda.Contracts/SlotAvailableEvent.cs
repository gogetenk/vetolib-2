using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Domain event raised when an appointment is cancelled and a slot becomes available.
/// Consumed by the waitlist handler to notify matching entries.
/// </summary>
public record SlotAvailableEvent(
    Guid ClinicId,
    Guid VeterinarianId,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes) : IDomainEvent;
