namespace Vetolib.Messaging.Contracts;

public record TriageStatsDto(
    double AverageFirstResponseTime,
    IReadOnlyList<CategoryCountDto> MessagesByCategory,
    double AiTriageAccuracy,
    IReadOnlyList<DailyVolumeDto> VolumePerDay,
    double ConversionRateToAppointment
);

public record CategoryCountDto(
    MessageCategory Category,
    int Count
);

public record DailyVolumeDto(
    DateOnly Date,
    int Count
);
