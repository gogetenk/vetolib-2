namespace Vetolib.Breeding.Contracts;

public record CreateLitterRequest(
    Guid MotherPatientId,
    Guid? FatherPatientId,
    string? ExternalFatherName,
    DateOnly BirthDate,
    int BornCount,
    int AliveCount,
    string? Notes);
