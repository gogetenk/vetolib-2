using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Colic is the leading cause of equine death. A combination of weight loss,
/// decreased appetite, and behavioral changes (rolling, pawing, looking at flank)
/// should trigger an urgent colic warning for early intervention.
/// </summary>
internal class HorseColicRiskRule : IHealthAlertRule
{
    public string RuleId => "HORSE_COLIC_RISK";

    private static readonly string[] ColicKeywords =
    [
        "colic", "abdominal pain", "rolling", "pawing", "flank watching",
        "decreased appetite", "anorexia", "lethargy", "bloating", "no gut sounds"
    ];

    private const decimal WeightLossThresholdPercent = 5m;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Horse)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var hasColicSign = ColicKeywords.Any(k => patient.HasDiagnosisContaining(k));
        var hasWeightLoss = DetectRecentWeightLoss(patient);

        // Need at least one behavioral/clinical sign
        if (!hasColicSign && !hasWeightLoss)
            return [];

        // Already being treated for colic
        if (patient.HasDiagnosisContaining("colic treatment") || patient.HasDiagnosisContaining("colic resolved"))
            return [];

        var severity = hasColicSign && hasWeightLoss
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = hasColicSign && hasWeightLoss ? 80 : 60;

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.HorseColicRisk,
            severity,
            "Colic risk indicators detected",
            $"{patient.Name} shows signs consistent with colic risk (leading cause of equine death). " +
            "Early detection and intervention are critical to prevent progression.",
            "Perform immediate physical exam including gut sounds auscultation, rectal exam, and nasogastric intubation if indicated. Monitor vital signs closely.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static bool DetectRecentWeightLoss(PatientAlertContext patient)
    {
        if (patient.WeightHistory.Count < 2)
            return false;

        var sorted = patient.WeightHistory.OrderByDescending(w => w.RecordedAt).ToList();
        var latest = sorted[0];
        var previous = sorted[1];

        if (previous.WeightKg == 0)
            return false;

        var changePercent = ((latest.WeightKg - previous.WeightKg) / previous.WeightKg) * 100m;
        return changePercent <= -WeightLossThresholdPercent;
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
