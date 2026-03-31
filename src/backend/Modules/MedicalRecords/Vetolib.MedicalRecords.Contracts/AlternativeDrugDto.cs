namespace Vetolib.MedicalRecords.Contracts;

public record AlternativeDrugDto(
    Guid DrugId,
    string Name,
    string ActiveIngredient,
    DrugCategory Category,
    string FormulationType,
    bool IsGeneric,
    PriceIndicator PriceIndicator,
    bool HasInteraction);
