using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Contracts;

public record PedigreeNodeDto(
    Guid PatientId,
    string Name,
    Species Species,
    string Breed,
    Sex Sex,
    string? RegistryNumber,
    PedigreeNodeDto? Mother,
    PedigreeNodeDto? Father);
