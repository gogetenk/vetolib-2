using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AVDC: Tooth resorption (formerly FORL) affects 60-80% of cats over 5 years.
/// Cats over 5 years with no dental exam in 12+ months should be flagged.
/// Often painful but cats hide signs — proactive screening is essential.
/// </summary>
internal class CatDentalResorptionRule : IHealthAlertRule
{
    public string RuleId => "CAT_DENTAL_RESORPTION";

    private const int MinAgeYears = 5;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Cat)
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("dental exam") ||
            patient.HasDiagnosisContaining("dental radiograph") ||
            patient.HasDiagnosisContaining("tooth resorption") ||
            patient.HasDiagnosisContaining("FORL"))
            return [];

        var severity = patient.AgeInYears >= 8
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 40 + (patient.AgeInYears - MinAgeYears) * 8);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CatDentalResorption,
            severity,
            "Dental exam for tooth resorption recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old cat with no recent dental exam. Tooth resorption affects 60-80% of cats over 5 years and is often painful but hidden. Dental radiographs are required for diagnosis.",
            "Schedule dental examination with full-mouth radiographs under anesthesia.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
