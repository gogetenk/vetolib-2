namespace Vetolib.Breeding.Contracts;

public record LitterDto(
    Guid Id,
    Guid MotherPatientId,
    Guid? FatherPatientId,
    string? ExternalFatherName,
    DateOnly BirthDate,
    int BornCount,
    int AliveCount,
    string? Notes,
    IReadOnlyList<LitterOffspringDto> Offspring);
