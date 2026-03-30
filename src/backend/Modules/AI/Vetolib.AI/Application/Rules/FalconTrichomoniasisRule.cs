using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Trichomoniasis (frounce) is a common protozoal disease in falcons,
/// especially those fed live or freshly caught pigeons. Crop and throat
/// symptoms should trigger screening.
/// </summary>
internal class FalconTrichomoniasisRule : IHealthAlertRule
{
    public string RuleId => "FALCON_TRICHOMONIASIS";

    private static readonly string[] TrichKeywords =
    [
        "trichomoniasis", "frounce", "crop lesion", "throat lesion",
        "oral plaque", "caseous", "crop stasis", "regurgitation",
        "dysphagia", "inappetence"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Already diagnosed and treated
        if (patient.HasDiagnosisContaining("trichomoniasis treated") || patient.HasDiagnosisContaining("frounce resolved"))
            return [];

        var hasTrichSign = TrichKeywords.Any(k => patient.HasDiagnosisContaining(k));

        if (!hasTrichSign)
            return [];

        var riskScore = 70;
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconTrichomoniasis,
            HealthAlertSeverity.High,
            "Trichomoniasis screening recommended",
            $"{patient.Name} presents with crop/throat symptoms consistent with trichomoniasis (frounce). This protozoal infection can be fatal in falcons if untreated.",
            "Perform crop wash and wet mount microscopy. Begin metronidazole or carnidazole treatment if confirmed.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
