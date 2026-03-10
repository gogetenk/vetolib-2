namespace Vetolib.Messaging.Contracts;

public record UpdateMessagingHoursRequest(
    IReadOnlyList<MessagingHoursDayRequest> Days
);

public record MessagingHoursDayRequest(
    int DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsClosed
);
