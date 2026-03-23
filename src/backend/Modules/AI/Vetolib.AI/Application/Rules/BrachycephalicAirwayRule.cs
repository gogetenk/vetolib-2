using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Brachycephalic breeds are at risk of Brachycephalic Obstructive Airway Syndrome (BOAS).
/// Screen from age 1+ with focus on respiratory symptoms.
/// </summary>
internal class BrachycephalicAirwayRule : IHealthAlertRule
{
    public string RuleId => "BRACHYCEPHALIC_AIRWAY";

    private const int MinAgeYears = 1;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (!BreedRiskData.IsBreedInSpeciesMap(patient.Species, patient.Breed, BreedRiskData.BrachycephalicBreeds))
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("brachycephalic") || patient.HasDiagnosisContaining("BOAS") || patient.HasDiagnosisContaining("airway"))
            return [];

        var riskScore = Math.Min(100, 45 + patient.AgeInYears * 3);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium,
            "Brachycephalic airway assessment recommended",
            $"{patient.Name} is a {patient.Breed} with brachycephalic conformation. BOAS assessment is recommended to evaluate airway function.",
            "Schedule BOAS grading assessment.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
