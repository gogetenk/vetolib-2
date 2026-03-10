namespace Vetolib.MedicalRecords.Contracts;

public record InteractionCheckResult(
    List<InteractionAlert> Alerts,
    List<DrugCatalogEntryDto> Alternatives);
