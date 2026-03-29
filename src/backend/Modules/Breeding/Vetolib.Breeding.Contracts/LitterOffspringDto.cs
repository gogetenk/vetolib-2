namespace Vetolib.Breeding.Contracts;

public record LitterOffspringDto(
    Guid Id,
    Guid LitterId,
    Guid PatientId,
    int? BirthOrder);
