namespace Vetolib.MedicalRecords.Contracts;

public record InteractionAlert(
    InteractionSeverity Severity,
    InteractionAlertType Type,
    string Message,
    List<Guid> AlternativeDrugIds);
