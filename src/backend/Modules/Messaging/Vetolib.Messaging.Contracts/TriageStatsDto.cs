namespace Vetolib.Messaging.Contracts;

public record TriageStatsDto(
    double AverageFirstResponseTimeMinutes,
    IReadOnlyList<CategoryCountDto> MessagesByCategory,
    double AiTriageAccuracyPercent,
    IReadOnlyList<DailyVolumeDto> VolumePerDay,
    double ConversionRatePercent
);

public record CategoryCountDto(
    MessageCategory Category,
    int Count
);

public record DailyVolumeDto(
    DateOnly Date,
    int Count
);
