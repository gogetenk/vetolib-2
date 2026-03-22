namespace Vetolib.Messaging.Contracts;

public record ClassificationAccuracyDto(
    double AccuracyRate,
    int TotalClassified,
    int TotalCorrected,
    IReadOnlyList<CategoryCorrectionDto> MostCommonCorrections
);

public record CategoryCorrectionDto(
    ClassifiedCategory FromCategory,
    ClassifiedCategory ToCategory,
    int Count
);
