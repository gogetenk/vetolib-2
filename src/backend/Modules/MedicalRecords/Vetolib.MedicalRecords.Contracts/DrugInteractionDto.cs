namespace Vetolib.MedicalRecords.Contracts;

public record DrugInteractionDto(
    Guid OtherDrugId,
    string OtherDrugName,
    InteractionSeverity Severity,
    string Description);
