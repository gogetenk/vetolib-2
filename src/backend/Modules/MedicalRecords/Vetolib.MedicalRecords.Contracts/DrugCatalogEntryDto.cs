namespace Vetolib.MedicalRecords.Contracts;

public record DrugCatalogEntryDto(
    Guid Id,
    string InnName,
    string DisplayName,
    DrugCategory Category,
    bool IsActive,
    Guid? ClinicId,
    IReadOnlyList<SpeciesContraindicationDto> SpeciesContraindications,
    IReadOnlyList<DrugInteractionDto> Interactions,
    IReadOnlyList<DosageGuidelineDto> DosageGuidelines);
