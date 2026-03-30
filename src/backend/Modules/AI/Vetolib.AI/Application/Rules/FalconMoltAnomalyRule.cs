using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Falcons typically molt between June and October. Molting outside this window
/// or a diagnosis mentioning abnormal/delayed/retained feathers suggests
/// stress, nutritional deficiency, or disease.
/// </summary>
internal class FalconMoltAnomalyRule : IHealthAlertRule
{
    public string RuleId => "FALCON_MOLT_ANOMALY";

    private const int NormalMoltStart = 6;  // June
    private const int NormalMoltEnd = 10;   // October

    private static readonly string[] MoltKeywords =
    [
        "molt", "moult", "feather loss", "pin feather",
        "stress bar", "retained feather", "feather abnormality"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var hasMoltRecord = MoltKeywords.Any(k => patient.HasDiagnosisContaining(k));

        if (!hasMoltRecord)
            return [];

        var now = DateTime.UtcNow;
        var isOutOfSeason = now.Month < NormalMoltStart || now.Month > NormalMoltEnd;

        // Also flag if there's an explicit anomaly keyword regardless of season
        var hasAnomalyKeyword = patient.HasDiagnosisContaining("stress bar")
            || patient.HasDiagnosisContaining("retained feather")
            || patient.HasDiagnosisContaining("feather abnormality")
            || patient.HasDiagnosisContaining("abnormal molt")
            || patient.HasDiagnosisContaining("abnormal moult");

        if (!isOutOfSeason && !hasAnomalyKeyword)
            return [];

        var severity = hasAnomalyKeyword
            ? HealthAlertSeverity.Medium
            : HealthAlertSeverity.Low;

        var riskScore = hasAnomalyKeyword ? 55 : 35;

        var description = isOutOfSeason
            ? $"{patient.Name} is showing molt-related signs outside the normal June-October window. This may indicate stress, nutritional deficiency, or underlying disease."
            : $"{patient.Name} has feather abnormalities that may indicate stress, nutritional deficiency, or disease.";

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconMoltAnomaly,
            severity,
            "Molt timing or feather anomaly detected",
            description,
            "Review diet and husbandry. Consider blood panel for nutritional deficiencies and thyroid function.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
