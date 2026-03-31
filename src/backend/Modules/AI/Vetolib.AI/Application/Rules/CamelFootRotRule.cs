using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Foot rot (infectious pododermatitis) is common in racing camels due to
/// repeated stress on foot pads from sand track training. Detection of foot
/// swelling, lameness, or infection signs should trigger early intervention.
/// </summary>
internal class CamelFootRotRule : IHealthAlertRule
{
    public string RuleId => "CAMEL_FOOT_ROT";

    private static readonly string[] FootRotKeywords =
    [
        "foot rot", "foot swelling", "foot infection", "lameness",
        "pododermatitis", "foot abscess", "sole ulcer", "foot pad",
        "interdigital", "hoof crack"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Camel)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var hasFootRotSign = FootRotKeywords.Any(k => patient.HasDiagnosisContaining(k));

        if (!hasFootRotSign)
            return [];

        // Already being treated
        if (patient.HasDiagnosisContaining("foot rot treatment") || patient.HasDiagnosisContaining("pododermatitis resolved"))
            return [];

        var riskScore = 65;
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CamelFootRot,
            HealthAlertSeverity.Medium,
            "Foot rot follow-up required",
            $"{patient.Name} has signs of foot rot (infectious pododermatitis). This is common in racing camels and can end a racing career if untreated.",
            "Examine foot pads thoroughly. Clean and debride affected areas. Consider topical and systemic antibiotics. Restrict training until resolved.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
