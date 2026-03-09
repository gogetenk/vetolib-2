namespace Vetolib.Agenda.Contracts;

public record CreateAppointmentRequest(
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Reason);
