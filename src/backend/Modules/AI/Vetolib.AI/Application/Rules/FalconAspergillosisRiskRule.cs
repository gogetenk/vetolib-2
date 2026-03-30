using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Aspergillosis is the most common and deadly fungal infection in falcons.
/// Weight loss combined with respiratory signs (dyspnea, tail bob, voice change)
/// should trigger an urgent screening recommendation.
/// </summary>
internal class FalconAspergillosisRiskRule : IHealthAlertRule
{
    public string RuleId => "FALCON_ASPERGILLOSIS_RISK";

    private static readonly string[] RespiratoryKeywords =
    [
        "respiratory", "dyspnea", "tail bob", "voice change",
        "wheezing", "open-mouth breathing", "nasal discharge", "cough"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Already diagnosed and treated
        if (patient.HasDiagnosisContaining("aspergillosis"))
            return [];

        var hasRespiratorySign = RespiratoryKeywords.Any(k => patient.HasDiagnosisContaining(k));
        var hasWeightLoss = HasRecentWeightLoss(patient);

        // Both weight loss AND respiratory signs required
        if (!hasRespiratorySign || !hasWeightLoss)
            return [];

        var riskScore = 75;
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconAspergillosisRisk,
            HealthAlertSeverity.High,
            "Aspergillosis screening recommended",
            $"{patient.Name} presents with weight loss and respiratory signs. Aspergillosis is the leading cause of fungal disease in falcons and can be fatal if untreated.",
            "Schedule endoscopy, CT scan, and Aspergillus antigen testing. Consider prophylactic antifungal therapy.",
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
        return changePercent < -5m; // Any >5% loss is concerning
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
