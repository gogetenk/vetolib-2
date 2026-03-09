namespace Vetolib.Agenda.Contracts;

public record UpdateAppointmentStatusRequest(
    AppointmentStatus NewStatus,
    string? Reason);
