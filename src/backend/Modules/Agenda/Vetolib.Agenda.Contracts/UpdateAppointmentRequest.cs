namespace Vetolib.Agenda.Contracts;

public record UpdateAppointmentRequest(
    DateOnly? Date,
    TimeOnly? StartTime,
    int? DurationMinutes,
    Guid? VeterinarianId,
    string? VeterinarianName,
    string? Reason,
    string? Notes);
