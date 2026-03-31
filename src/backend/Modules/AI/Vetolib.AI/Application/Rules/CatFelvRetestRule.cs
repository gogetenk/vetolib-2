using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAFP Retrovirus Guidelines: Outdoor cats (or cats with outdoor access) should be
/// tested for FeLV annually. FeLV is transmitted through saliva, nasal secretions,
/// and is the leading viral cause of cancer in cats.
/// </summary>
internal class CatFelvRetestRule : IHealthAlertRule
{
    public string RuleId => "CAT_FELV_RETEST";

    private static readonly string[] OutdoorKeywords =
    [
        "outdoor", "outside", "free-roaming", "indoor-outdoor",
        "stray", "feral", "barn cat"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Cat)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("FeLV test") ||
            patient.HasDiagnosisContaining("FeLV negative") ||
            patient.HasDiagnosisContaining("FeLV positive") ||
            patient.HasDiagnosisContaining("feline leukemia test"))
            return [];

        // Check if there's evidence of outdoor access in records
        var hasOutdoorAccess = OutdoorKeywords.Any(k => patient.HasDiagnosisContaining(k));
        if (!hasOutdoorAccess)
            return [];

        var severity = HealthAlertSeverity.Medium;
        var riskScore = Math.Min(100, 55 + patient.AgeInYears * 3);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CatFelvRetest,
            severity,
            "FeLV retest recommended",
            $"{patient.Name} has outdoor access and no recent FeLV test on file. Annual FeLV testing is recommended for cats with outdoor exposure per AAFP guidelines.",
            "Schedule FeLV SNAP test. If positive, confirm with IFA or PCR.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
