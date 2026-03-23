using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Detect significant weight changes (>10% gain or loss over 3 months)
/// which may indicate underlying disease.
/// </summary>
internal class WeightTrendRule : IHealthAlertRule
{
    public string RuleId => "WEIGHT_TREND";

    private const decimal SignificantChangePercent = 10m;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.WeightHistory.Count < 2)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var sortedWeights = patient.WeightHistory
            .OrderByDescending(w => w.RecordedAt)
            .ToList();

        var latest = sortedWeights[0];
        var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
        var oldEntry = sortedWeights.FirstOrDefault(w => w.RecordedAt <= threeMonthsAgo);

        if (oldEntry is null)
            return [];

        var changePercent = ((latest.WeightKg - oldEntry.WeightKg) / oldEntry.WeightKg) * 100m;

        if (Math.Abs(changePercent) < SignificantChangePercent)
            return [];

        var direction = changePercent > 0 ? "gain" : "loss";
        var severity = Math.Abs(changePercent) >= 20m
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, (int)(30 + Math.Abs(changePercent) * 2));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.WeightTrend,
            severity,
            $"Significant weight {direction} detected",
            $"{patient.Name} has experienced a {Math.Abs(changePercent):F1}% weight {direction} over the past 3 months ({oldEntry.WeightKg:F1} kg to {latest.WeightKg:F1} kg). This may indicate an underlying condition.",
            "Schedule weight assessment and metabolic panel.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
