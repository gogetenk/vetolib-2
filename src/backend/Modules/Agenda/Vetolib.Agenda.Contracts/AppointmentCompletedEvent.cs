using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Domain event raised when an appointment is completed.
/// Consumed by the follow-up handler to schedule automatic follow-up appointments.
/// </summary>
public record AppointmentCompletedEvent(
    Guid ClinicId,
    Guid AppointmentId,
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    string? OwnerEmail,
    DateOnly Date,
    string? Reason) : IDomainEvent;
