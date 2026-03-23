namespace Vetolib.AI.Contracts;

public record HealthAlertDto(
    Guid Id,
    Guid PatientId,
    HealthAlertType AlertType,
    HealthAlertSeverity Severity,
    string Title,
    string Description,
    string? RecommendedAction,
    string RuleId,
    int RiskScore,
    HealthAlertStatus Status,
    DateTime GeneratedAt,
    DateTime? DismissedAt,
    string? DismissedReason,
    string? DismissedByName,
    DateTime? AcknowledgedAt,
    Guid? ConvertedToAppointmentId);
