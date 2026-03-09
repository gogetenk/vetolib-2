namespace Vetolib.Agenda.Contracts;

public record SuggestSlotRequest(
    string ConsultationType,
    DateOnly PreferredDate,
    TimeOnly PreferredTime,
    Guid? PreferredVeterinarianId = null,
    int? DurationMinutes = null);
