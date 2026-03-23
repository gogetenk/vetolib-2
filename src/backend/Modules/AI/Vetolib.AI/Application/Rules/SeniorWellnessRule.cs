using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA/WSAVA: Senior patients should have biannual wellness exams including
/// CBC, chemistry panel, thyroid, and urinalysis.
/// </summary>
internal class SeniorWellnessRule : IHealthAlertRule
{
    public string RuleId => "SENIOR_WELLNESS";

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (!BreedRiskData.SeniorAgeThresholdYears.TryGetValue(patient.Species, out var threshold))
            return [];

        if (patient.AgeInYears < threshold)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // If there's been a recent wellness exam, skip
        if (patient.HasDiagnosisContaining("wellness") || patient.HasDiagnosisContaining("senior check"))
            return [];

        var severity = patient.AgeInYears >= threshold + 5
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 30 + (patient.AgeInYears - threshold) * 8);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.SeniorWellness,
            severity,
            "Senior wellness exam due",
            $"{patient.Name} is {patient.AgeInYears} years old ({patient.Species}). Biannual senior wellness exam with CBC, chemistry panel, thyroid screen, and urinalysis is recommended.",
            "Schedule comprehensive senior wellness examination.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
