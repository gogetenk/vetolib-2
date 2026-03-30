using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Bumblefoot (pododermatitis) is extremely common in captive falcons.
/// Detection of foot swelling, lesions, or related symptoms should trigger
/// early intervention to prevent progression.
/// </summary>
internal class FalconBumblefootRule : IHealthAlertRule
{
    public string RuleId => "FALCON_BUMBLEFOOT";

    private static readonly string[] BumblefootKeywords =
    [
        "bumblefoot", "pododermatitis", "foot swelling", "foot lesion",
        "plantar", "foot abscess", "metatarsal pad", "foot sore"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var hasBumblefootSign = BumblefootKeywords.Any(k => patient.HasDiagnosisContaining(k));

        if (!hasBumblefootSign)
            return [];

        // Already being actively treated
        if (patient.HasDiagnosisContaining("bumblefoot treatment") || patient.HasDiagnosisContaining("pododermatitis resolved"))
            return [];

        var riskScore = 65;
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconBumblefoot,
            HealthAlertSeverity.Medium,
            "Bumblefoot follow-up required",
            $"{patient.Name} has signs of bumblefoot (pododermatitis). Early-stage bumblefoot is treatable but can progress to serious infection if untreated.",
            "Assess foot pads for lesions. Grade bumblefoot severity (I-V). Adjust perching surfaces and consider topical or systemic treatment.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
