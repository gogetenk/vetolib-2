using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA Dental Care Guidelines: Dogs and cats should have dental prophylaxis
/// annually from age 2+. Small breeds are especially prone to periodontal disease.
/// </summary>
internal class DentalProphylaxisRule : IHealthAlertRule
{
    public string RuleId => "DENTAL_PROPHYLAXIS";

    private const int MinAgeYears = 2;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species is not (Species.Dog or Species.Cat))
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("dental") || patient.HasDiagnosisContaining("prophylaxis") ||
            patient.HasDiagnosisContaining("teeth") || patient.HasDiagnosisContaining("periodontal"))
            return [];

        var severity = patient.AgeInYears >= 6
            ? HealthAlertSeverity.Medium
            : HealthAlertSeverity.Low;

        var riskScore = Math.Min(100, 25 + patient.AgeInYears * 5);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.DentalProphylaxis,
            severity,
            "Dental prophylaxis recommended",
            $"{patient.Name} is {patient.AgeInYears} years old with no recent dental records. Annual dental prophylaxis is recommended per AAHA guidelines to prevent periodontal disease.",
            "Schedule dental examination and prophylaxis under anesthesia.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
