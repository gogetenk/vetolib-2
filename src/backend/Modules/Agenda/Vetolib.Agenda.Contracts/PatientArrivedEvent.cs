using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Domain event raised when a patient arrives in the waiting room.
/// Consumed by the notification handler to alert the assigned veterinarian.
/// </summary>
public record PatientArrivedEvent(
    Guid ClinicId,
    Guid AppointmentId,
    Guid VeterinarianId,
    string VeterinarianName,
    string PatientName,
    string OwnerName,
    TimeOnly AppointmentTime) : IDomainEvent;
