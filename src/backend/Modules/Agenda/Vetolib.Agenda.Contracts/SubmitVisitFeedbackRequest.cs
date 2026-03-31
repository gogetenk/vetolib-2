namespace Vetolib.Agenda.Contracts;

public record SubmitVisitFeedbackRequest(
    int Rating,
    string? Comment,
    bool IsPublic);
