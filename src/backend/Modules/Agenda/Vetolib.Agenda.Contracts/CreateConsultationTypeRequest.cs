namespace Vetolib.Agenda.Contracts;

public record CreateConsultationTypeRequest(
    string Name,
    int DurationMinutes,
    int SortOrder = 0,
    bool RequiresVetSelection = false);
