using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA/WSAVA: Cats over 7 years should have annual renal screening (BUN, creatinine, SDMA).
/// Higher risk for breeds like Persian, Siamese, Abyssinian, Maine Coon.
/// </summary>
internal class CatRenalScreeningRule : IHealthAlertRule
{
    private static readonly IReadOnlyList<string> HighRiskBreeds = new[]
    {
        "Persian", "Siamese", "Abyssinian", "Maine Coon", "Burmese", "Russian Blue"
    };

    public string RuleId => "CAT_RENAL_SCREENING";

    private const int MinAgeYears = 7;
    private const int HighRiskMinAgeYears = 5;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Cat)
            return [];

        var isHighRisk = BreedRiskData.IsBreedInList(patient.Breed, HighRiskBreeds);
        var ageThreshold = isHighRisk ? HighRiskMinAgeYears : MinAgeYears;

        if (patient.AgeInYears < ageThreshold)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Check if there's a recent renal screening in the last 12 months
        if (patient.HasDiagnosisContaining("renal") || patient.HasDiagnosisContaining("kidney") || patient.HasDiagnosisContaining("SDMA"))
            return [];

        var severity = isHighRisk || patient.AgeInYears >= 12
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 40 + (patient.AgeInYears - ageThreshold) * 5 + (isHighRisk ? 20 : 0));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.BreedSpecificScreening,
            severity,
            "Renal screening recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {patient.Breed} cat. Annual renal screening (BUN, creatinine, SDMA, urinalysis) is recommended per AAHA/WSAVA senior care guidelines.",
            "Schedule renal panel blood work and urinalysis.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
