using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAFP: Hyperthyroidism is the most common endocrine disorder in cats over 10 years.
/// Classic triad: weight loss + increased appetite + age > 10 years.
/// </summary>
internal class CatHyperthyroidismRule : IHealthAlertRule
{
    public string RuleId => "CAT_HYPERTHYROIDISM";

    private const int MinAgeYears = 10;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Cat)
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("hyperthyroid") ||
            patient.HasDiagnosisContaining("thyroid") ||
            patient.HasDiagnosisContaining("T4"))
            return [];

        var hasWeightLoss = HasRecentWeightLoss(patient);
        var hasIncreasedAppetite = patient.HasDiagnosisContaining("increased appetite") ||
                                   patient.HasDiagnosisContaining("polyphagia") ||
                                   patient.HasDiagnosisContaining("ravenous");

        // Need weight loss + increased appetite for high confidence
        if (!hasWeightLoss && !hasIncreasedAppetite)
            return [];

        var severity = hasWeightLoss && hasIncreasedAppetite
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 50 + (hasWeightLoss ? 20 : 0) + (hasIncreasedAppetite ? 20 : 0));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CatHyperthyroidism,
            severity,
            "Hyperthyroidism screening recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old cat showing signs consistent with hyperthyroidism{(hasWeightLoss ? " (weight loss)" : "")}{(hasIncreasedAppetite ? " (increased appetite)" : "")}. This is the most common endocrine disorder in senior cats.",
            "Schedule total T4 blood test. If elevated, consider free T4 and thyroid scintigraphy.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static bool HasRecentWeightLoss(PatientAlertContext patient)
    {
        if (patient.WeightHistory.Count < 2)
            return false;

        var sorted = patient.WeightHistory.OrderByDescending(w => w.RecordedAt).ToList();
        var latest = sorted[0];
        var previous = sorted[1];

        if (previous.WeightKg <= 0)
            return false;

        var changePercent = ((latest.WeightKg - previous.WeightKg) / previous.WeightKg) * 100m;
        return changePercent < -5m;
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
