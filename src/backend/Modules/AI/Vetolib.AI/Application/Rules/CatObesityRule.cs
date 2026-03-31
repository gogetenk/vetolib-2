using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAFP/AAHA: Obesity in cats (typically > 5.5kg for domestic shorthair) is associated with
/// diabetes mellitus (4x risk), hepatic lipidosis, lower urinary tract disease, and osteoarthritis.
/// Breed-adjusted thresholds for larger breeds like Maine Coon.
/// </summary>
internal class CatObesityRule : IHealthAlertRule
{
    public string RuleId => "CAT_OBESITY";

    private static readonly IReadOnlyDictionary<string, decimal> BreedWeightThresholdKg =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["Domestic Shorthair"] = 5.5m,
            ["Domestic Longhair"] = 5.5m,
            ["Domestic Medium Hair"] = 5.5m,
            ["Siamese"] = 5.0m,
            ["Persian"] = 6.0m,
            ["Maine Coon"] = 9.0m,
            ["Ragdoll"] = 8.0m,
            ["British Shorthair"] = 7.0m,
            ["Norwegian Forest Cat"] = 8.0m,
            ["Bengal"] = 6.5m,
            ["Abyssinian"] = 5.0m,
            ["Russian Blue"] = 5.5m,
            ["Burmese"] = 5.5m,
            ["Sphynx"] = 5.0m
        };

    private const decimal DefaultWeightThresholdKg = 5.5m;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Cat)
            return [];

        if (!patient.WeightKg.HasValue || patient.WeightKg.Value <= 0)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var threshold = GetBreedThreshold(patient.Breed);
        if (patient.WeightKg.Value <= threshold)
            return [];

        var overweightPercent = ((patient.WeightKg.Value - threshold) / threshold) * 100m;
        var severity = overweightPercent >= 30m
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, (int)(40 + overweightPercent * 1.5m));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CatObesity,
            severity,
            "Feline weight management recommended",
            $"{patient.Name} is a {patient.Breed} weighing {patient.WeightKg.Value:F1}kg (threshold: {threshold:F1}kg). Feline obesity increases diabetes risk 4x and is associated with hepatic lipidosis and urinary tract disease.",
            "Schedule nutritional consultation. Consider metabolic panel and thyroid screening. Recommend measured feeding and environmental enrichment.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static decimal GetBreedThreshold(string breed)
    {
        foreach (var kvp in BreedWeightThresholdKg)
        {
            if (breed.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return DefaultWeightThresholdKg;
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
