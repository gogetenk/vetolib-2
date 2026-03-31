namespace Vetolib.Agenda.Contracts;

public record VisitFeedbackStatsDto(
    double AverageRating,
    int TotalCount,
    int CountStar1,
    int CountStar2,
    int CountStar3,
    int CountStar4,
    int CountStar5,
    double NpsScore);
