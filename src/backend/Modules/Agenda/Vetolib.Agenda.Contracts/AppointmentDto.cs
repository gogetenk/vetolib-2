namespace Vetolib.Agenda.Contracts;

public record AppointmentDto(
    Guid Id,
    Guid ClinicId,
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes,
    TimeOnly EndTime,
    AppointmentStatus Status,
    string? Reason);
