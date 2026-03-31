using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA Weight Management Guidelines: Dogs with weight exceeding 20% above breed average
/// are at risk for obesity-related diseases (diabetes, joint disease, reduced lifespan).
/// Uses approximate breed weight ranges.
/// </summary>
internal class DogObesityRiskRule : IHealthAlertRule
{
    public string RuleId => "DOG_OBESITY_RISK";

    // Approximate upper-normal weight in kg by breed (males/females averaged)
    private static readonly IReadOnlyDictionary<string, decimal> BreedAverageWeightKg =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["Labrador Retriever"] = 32m,
            ["Golden Retriever"] = 32m,
            ["German Shepherd"] = 34m,
            ["Bulldog"] = 23m,
            ["French Bulldog"] = 12m,
            ["Beagle"] = 11m,
            ["Poodle"] = 27m,
            ["Rottweiler"] = 50m,
            ["Dachshund"] = 10m,
            ["Corgi"] = 12m,
            ["Yorkshire Terrier"] = 3.2m,
            ["Boxer"] = 30m,
            ["Pug"] = 7.5m,
            ["Cavalier King Charles Spaniel"] = 7m,
            ["Doberman"] = 40m,
            ["Shih Tzu"] = 6.5m,
            ["Chihuahua"] = 2.5m,
            ["Miniature Schnauzer"] = 7m,
            ["Great Dane"] = 60m,
            ["Siberian Husky"] = 23m,
            ["Cocker Spaniel"] = 13m,
        };

    private const decimal DefaultAverageWeightKg = 20m;
    private const decimal ObesityThresholdPercent = 20m;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Dog)
            return [];

        if (!patient.WeightKg.HasValue || patient.WeightKg.Value <= 0)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var breedAverage = GetBreedAverageWeight(patient.Breed);
        var threshold = breedAverage * (1 + ObesityThresholdPercent / 100m);

        if (patient.WeightKg.Value <= threshold)
            return [];

        var overweightPercent = ((patient.WeightKg.Value - breedAverage) / breedAverage) * 100m;
        var severity = overweightPercent >= 40m
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, (int)(30 + overweightPercent));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.DogObesityRisk,
            severity,
            "Weight management recommended",
            $"{patient.Name} is a {patient.Breed} weighing {patient.WeightKg.Value:F1}kg, which is {overweightPercent:F0}% above breed average ({breedAverage:F1}kg). Obesity increases risk of diabetes, joint disease, and shortened lifespan.",
            "Schedule nutritional consultation and weight management plan. Consider metabolic panel.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static decimal GetBreedAverageWeight(string breed)
    {
        foreach (var kvp in BreedAverageWeightKg)
        {
            if (breed.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return DefaultAverageWeightKg;
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
