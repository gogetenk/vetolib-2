namespace Vetolib.Agenda.Contracts;

public record AppointmentHistoryDto(
    AppointmentStatus Status,
    DateOnly Date,
    int DurationMinutes,
    string? ConsultationType,
    bool WasNoShow,
    bool WasReminderSent);
