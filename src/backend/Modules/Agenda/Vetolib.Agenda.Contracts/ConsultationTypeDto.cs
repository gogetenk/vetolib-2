namespace Vetolib.Agenda.Contracts;

public record ConsultationTypeDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    bool IsActive,
    int SortOrder,
    bool RequiresVetSelection);
