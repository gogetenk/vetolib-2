using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Pre-race fitness certification for UAE racing camels. Checks that weight
/// is within normal racing range, no recent illness, and vaccination is current.
/// Racing season in UAE is typically October-April.
/// </summary>
internal class CamelRacingFitnessRule : IHealthAlertRule
{
    public string RuleId => "CAMEL_RACING_FITNESS";

    // Normal racing camel weight range in kg
    private const decimal MinRacingWeightKg = 350m;
    private const decimal MaxRacingWeightKg = 700m;

    private static readonly string[] IllnessKeywords =
    [
        "illness", "infection", "fever", "diarrhea", "colic",
        "abscess", "pneumonia", "sepsis", "mastitis"
    ];

    private static readonly string[] VaccinationKeywords =
    [
        "vaccination", "vaccine", "booster", "immunization"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Camel)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Only relevant during racing season (October-April)
        var now = DateTime.UtcNow;
        if (now.Month > 4 && now.Month < 10)
            return [];

        // Camel must be at least 2 years old for racing
        if (patient.AgeInYears < 2)
            return [];

        var issues = new List<string>();
        var severity = HealthAlertSeverity.Medium;

        // Check weight range
        if (patient.WeightKg.HasValue)
        {
            if (patient.WeightKg.Value < MinRacingWeightKg)
                issues.Add("underweight for racing");
            else if (patient.WeightKg.Value > MaxRacingWeightKg)
                issues.Add("overweight for racing");
        }

        // Check recent illness
        var hasRecentIllness = IllnessKeywords.Any(k => patient.HasDiagnosisContaining(k));
        if (hasRecentIllness)
        {
            issues.Add("recent illness on record");
            severity = HealthAlertSeverity.High;
        }

        // Check vaccination status
        var hasVaccination = VaccinationKeywords.Any(k => patient.HasDiagnosisContaining(k));
        if (!hasVaccination)
            issues.Add("no vaccination record found");

        if (issues.Count == 0)
            return [];

        var riskScore = severity == HealthAlertSeverity.High ? 70 : 55;
        var issueList = string.Join(", ", issues);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CamelRacingFitness,
            severity,
            "Pre-race fitness concerns detected",
            $"{patient.Name} has fitness concerns for racing: {issueList}. Pre-race veterinary clearance is required by UAE racing regulations.",
            "Perform comprehensive pre-race examination. Address identified issues before issuing fitness certificate. Verify vaccination records are current.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
