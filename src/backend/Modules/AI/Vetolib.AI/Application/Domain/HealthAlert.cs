using Ardalis.Result;
using Vetolib.AI.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.AI.Application.Domain;

internal class HealthAlert : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public HealthAlertType AlertType { get; private set; }
    public HealthAlertSeverity Severity { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? RecommendedAction { get; private set; }
    public string RuleId { get; private set; } = string.Empty;
    public int RiskScore { get; private set; }
    public HealthAlertStatus Status { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public DateTime? DismissedAt { get; private set; }
    public string? DismissedReason { get; private set; }
    public string? DismissedByName { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? ConvertedToAppointmentId { get; private set; }

    // EF Core constructor
    private HealthAlert() { }

    public static Result<HealthAlert> Create(
        Guid clinicId,
        Guid patientId,
        HealthAlertType alertType,
        HealthAlertSeverity severity,
        string title,
        string description,
        string? recommendedAction,
        string ruleId,
        int riskScore)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ClinicId), "ClinicId is required."));
        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(PatientId), "PatientId is required."));
        if (string.IsNullOrWhiteSpace(title))
            errors.Add(new ValidationError(nameof(Title), "Title is required."));
        if (string.IsNullOrWhiteSpace(description))
            errors.Add(new ValidationError(nameof(Description), "Description is required."));
        if (string.IsNullOrWhiteSpace(ruleId))
            errors.Add(new ValidationError(nameof(RuleId), "RuleId is required."));
        if (riskScore is < 0 or > 100)
            errors.Add(new ValidationError(nameof(RiskScore), "RiskScore must be between 0 and 100."));

        if (errors.Count > 0)
            return Result<HealthAlert>.Invalid(errors);

        return Result<HealthAlert>.Success(new HealthAlert
        {
            ClinicId = clinicId,
            PatientId = patientId,
            AlertType = alertType,
            Severity = severity,
            Title = title,
            Description = description,
            RecommendedAction = recommendedAction,
            RuleId = ruleId,
            RiskScore = riskScore,
            Status = HealthAlertStatus.New,
            GeneratedAt = DateTime.UtcNow
        });
    }

    public Result Dismiss(string reason, string dismissedByName)
    {
        if (Status == HealthAlertStatus.Dismissed)
            return Result.Error("Alert is already dismissed.");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Invalid(new ValidationError(nameof(reason), "Dismiss reason is required."));

        if (string.IsNullOrWhiteSpace(dismissedByName))
            return Result.Invalid(new ValidationError(nameof(dismissedByName), "Dismissed by name is required."));

        Status = HealthAlertStatus.Dismissed;
        DismissedAt = DateTime.UtcNow;
        DismissedReason = reason;
        DismissedByName = dismissedByName;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Acknowledge()
    {
        if (Status != HealthAlertStatus.New)
            return Result.Error($"Cannot acknowledge alert in status '{Status}'.");

        Status = HealthAlertStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkScheduled(Guid appointmentId)
    {
        if (Status == HealthAlertStatus.Dismissed)
            return Result.Error("Cannot schedule a dismissed alert.");

        if (appointmentId == Guid.Empty)
            return Result.Invalid(new ValidationError(nameof(appointmentId), "AppointmentId is required."));

        Status = HealthAlertStatus.Scheduled;
        ConvertedToAppointmentId = appointmentId;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
