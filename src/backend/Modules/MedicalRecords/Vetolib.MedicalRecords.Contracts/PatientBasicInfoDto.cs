namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Lightweight patient info used by other modules for cross-module validation
/// (e.g., Breeding checks mother sex and species match).
/// </summary>
public record PatientBasicInfoDto(
    Guid PatientId,
    string Name,
    Species Species,
    Sex Sex);
