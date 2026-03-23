using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA/WSAVA: Breeds predisposed to cardiac disease should have cardiac screening
/// (auscultation, echocardiography) starting at breed-specific age thresholds.
/// </summary>
internal class CardiacBreedRule : IHealthAlertRule
{
    public string RuleId => "CARDIAC_BREED_SCREENING";

    private const int MinAgeYears = 3;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (!BreedRiskData.IsBreedInSpeciesMap(patient.Species, patient.Breed, BreedRiskData.CardiacRiskBreeds))
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("cardiac") || patient.HasDiagnosisContaining("heart") || patient.HasDiagnosisContaining("echocardiograph"))
            return [];

        var riskScore = Math.Min(100, 50 + (patient.AgeInYears - MinAgeYears) * 5);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium,
            "Cardiac screening recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {patient.Breed} with known breed predisposition to cardiac disease. Cardiac auscultation and echocardiography are recommended.",
            "Schedule cardiac screening with echocardiography.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
