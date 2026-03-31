using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA Senior Care Guidelines: Large breed dogs (>25kg) are senior at 7+ years,
/// small breed dogs (&lt;10kg) at 10+ years. Senior dogs without a blood panel in
/// 12+ months should be flagged for comprehensive screening.
/// </summary>
internal class DogSeniorScreeningRule : IHealthAlertRule
{
    public string RuleId => "DOG_SENIOR_SCREENING";

    private const int LargeBreedSeniorAge = 7;
    private const int SmallBreedSeniorAge = 10;
    private const decimal LargeBreedWeightThreshold = 25m;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Dog)
            return [];

        var seniorAge = GetSeniorAgeThreshold(patient);
        if (patient.AgeInYears < seniorAge)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("blood panel") ||
            patient.HasDiagnosisContaining("senior screen") ||
            patient.HasDiagnosisContaining("CBC") ||
            patient.HasDiagnosisContaining("chemistry panel"))
            return [];

        var yearsOverThreshold = patient.AgeInYears - seniorAge;
        var severity = yearsOverThreshold >= 3
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 40 + yearsOverThreshold * 10);

        var sizeCategory = IsLargeBreed(patient) ? "large breed" : "small breed";
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.DogSeniorScreening,
            severity,
            "Senior blood panel due",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {sizeCategory} {patient.Breed} with no recent blood panel on file. Annual comprehensive screening (CBC, chemistry, thyroid, urinalysis) is recommended for senior dogs.",
            "Schedule comprehensive senior blood panel including CBC, chemistry, thyroid, and urinalysis.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static int GetSeniorAgeThreshold(PatientAlertContext patient)
        => IsLargeBreed(patient) ? LargeBreedSeniorAge : SmallBreedSeniorAge;

    private static bool IsLargeBreed(PatientAlertContext patient)
        => patient.WeightKg.HasValue && patient.WeightKg.Value >= LargeBreedWeightThreshold;

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
