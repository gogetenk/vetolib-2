namespace Vetolib.Messaging.Contracts;

public record MessagingHoursDto(
    Guid Id,
    Guid ClinicId,
    int DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsClosed
);
