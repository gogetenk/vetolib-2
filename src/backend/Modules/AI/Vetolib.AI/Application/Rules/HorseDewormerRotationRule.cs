using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Parasite resistance is a growing concern in equine medicine.
/// Using the same dewormer class 3+ times consecutively increases resistance risk.
/// Modern equine parasitology recommends strategic deworming with fecal egg counts
/// and rotation of active ingredients.
/// </summary>
internal class HorseDewormerRotationRule : IHealthAlertRule
{
    public string RuleId => "HORSE_DEWORMER_ROTATION";

    private const int RepeatThreshold = 3;

    private static readonly string[] DewormerKeywords =
    [
        "ivermectin", "moxidectin", "fenbendazole", "pyrantel",
        "praziquantel", "oxibendazole", "dewormer", "anthelmintic"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Horse)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var dewormingRecords = patient.RecentRecords
            .Where(r => DewormerKeywords.Any(kw =>
                r.Diagnosis.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(r => r.ExaminedAt)
            .ToList();

        if (dewormingRecords.Count < RepeatThreshold)
            return [];

        // Check if the same dewormer class is used repeatedly
        var dominantDewormer = FindDominantDewormer(dewormingRecords);
        if (dominantDewormer is null)
            return [];

        var riskScore = 60;

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.HorseDewormerRotation,
            HealthAlertSeverity.Medium,
            "Dewormer rotation recommended",
            $"{patient.Name} has been treated with {dominantDewormer} in {RepeatThreshold}+ consecutive deworming records. " +
            "Repeated use of the same anthelmintic class increases parasite resistance risk.",
            "Perform fecal egg count (FEC) to assess parasite burden. Rotate to a different anthelmintic class. " +
            "Consider targeted selective treatment based on FEC results rather than routine deworming.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    internal static string? FindDominantDewormer(IReadOnlyList<MedicalRecordSummaryDto> records)
    {
        // Check the most recent N records for a single repeated dewormer
        var recentRecords = records.Take(RepeatThreshold).ToList();

        foreach (var dewormer in DewormerKeywords)
        {
            if (dewormer is "dewormer" or "anthelmintic")
                continue; // Skip generic terms

            var matchCount = recentRecords.Count(r =>
                r.Diagnosis.Contains(dewormer, StringComparison.OrdinalIgnoreCase));

            if (matchCount >= RepeatThreshold)
                return dewormer;
        }

        return null;
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
