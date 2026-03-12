namespace Vetolib.Agenda.Contracts;

public record UpdateConsultationTypeRequest(
    string Name,
    int DurationMinutes,
    int SortOrder,
    bool RequiresVetSelection);
