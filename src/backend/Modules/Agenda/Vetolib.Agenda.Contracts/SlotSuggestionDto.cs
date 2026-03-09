namespace Vetolib.Agenda.Contracts;

public record SlotSuggestionDto(
    Guid VeterinarianId,
    string VeterinarianName,
    DateOnly Date,
    TimeOnly StartTime,
    int EstimatedDurationMinutes,
    int Score,
    string Reasoning);

public record SlotSuggestionsResponse(IReadOnlyList<SlotSuggestionDto> Suggestions);
