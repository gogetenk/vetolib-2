namespace Vetolib.Agenda.Contracts;

public record CreateAppointmentSeriesRequest(
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    string? OwnerEmail,
    DateOnly StartDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Reason,
    RecurrenceFrequency Frequency,
    int Count);
