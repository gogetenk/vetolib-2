using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Breeds predisposed to diabetes mellitus should be monitored,
/// especially when overweight or middle-aged+.
/// </summary>
internal class DiabetesRiskRule : IHealthAlertRule
{
    public string RuleId => "DIABETES_RISK";

    private const int MinAgeYears = 5;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (!BreedRiskData.IsBreedInSpeciesMap(patient.Species, patient.Breed, BreedRiskData.DiabetesRiskBreeds))
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("diabetes") || patient.HasDiagnosisContaining("glucose") || patient.HasDiagnosisContaining("insulin"))
            return [];

        var riskScore = Math.Min(100, 35 + patient.AgeInYears * 4);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium,
            "Diabetes risk screening recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {patient.Breed} with breed predisposition to diabetes mellitus. Blood glucose and fructosamine screening are recommended.",
            "Schedule fasting blood glucose and fructosamine test.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
