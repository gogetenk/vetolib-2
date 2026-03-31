namespace Vetolib.Agenda.Contracts;

public record WaitingRoomAppointmentDto(
    Guid AppointmentId,
    string PatientName,
    string OwnerName,
    TimeOnly AppointmentTime,
    string VeterinarianName,
    DateTime WaitingRoomAt,
    int WaitMinutes);
