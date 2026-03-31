using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// ISFM/AAFP: Chronic Kidney Disease (CKD) is the #1 cause of death in cats.
/// Cats over 7 years should have annual kidney panels (BUN, creatinine, SDMA, urinalysis).
/// Distinct from CatRenalScreeningRule: this focuses specifically on CKD risk factors
/// including weight loss trend + age, not just age-based screening.
/// </summary>
internal class CatKidneyDiseaseScreeningRule : IHealthAlertRule
{
    public string RuleId => "CAT_KIDNEY_DISEASE_SCREENING";

    private const int MinAgeYears = 7;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Cat)
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("kidney panel") ||
            patient.HasDiagnosisContaining("CKD") ||
            patient.HasDiagnosisContaining("creatinine"))
            return [];

        var hasWeightLoss = HasRecentWeightLoss(patient);
        var severity = hasWeightLoss || patient.AgeInYears >= 12
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 45 + (patient.AgeInYears - MinAgeYears) * 5 + (hasWeightLoss ? 20 : 0));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CatKidneyDiseaseScreening,
            severity,
            "Kidney disease screening recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old cat with no recent kidney panel on file.{(hasWeightLoss ? " Recent weight loss detected, which is an early CKD indicator." : "")} CKD is the leading cause of feline mortality. Annual screening is recommended.",
            "Schedule kidney panel (BUN, creatinine, SDMA, urinalysis). Consider blood pressure measurement.",
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
