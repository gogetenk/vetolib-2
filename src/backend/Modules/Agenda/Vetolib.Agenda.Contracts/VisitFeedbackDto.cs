namespace Vetolib.Agenda.Contracts;

public record VisitFeedbackDto(
    Guid Id,
    Guid AppointmentId,
    int Rating,
    string? Comment,
    bool IsPublic,
    DateTime CreatedAt);
