using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Horses need annual dental examinations (floating) to prevent sharp enamel points,
/// hooks, and wave mouth that cause pain and weight loss. Unlike dogs/cats,
/// horse teeth continuously erupt and require regular maintenance.
/// </summary>
internal class HorseDentalCheckRule : IHealthAlertRule
{
    public string RuleId => "HORSE_DENTAL_CHECK";

    private const int DentalCheckIntervalMonths = 12;

    private static readonly string[] DentalKeywords =
    [
        "dental", "float", "teeth", "dentistry", "oral exam",
        "molar", "incisor", "wolf tooth", "quidding"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Horse)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Horses under 1 year don't need floating yet
        if (patient.AgeInYears < 1)
            return [];

        var dentalRecords = patient.RecentRecords
            .Where(r => DentalKeywords.Any(kw =>
                r.Diagnosis.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // If there are recent dental records, check the date
        if (dentalRecords.Count > 0)
        {
            var mostRecentDental = dentalRecords.Max(r => r.ExaminedAt);
            var monthsSinceDental = MonthsSince(mostRecentDental);

            if (monthsSinceDental < DentalCheckIntervalMonths)
                return [];
        }

        // No dental records at all, or last dental > 12 months ago
        var severity = patient.AgeInYears >= 15
            ? HealthAlertSeverity.High   // Senior horses need closer dental monitoring
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 40 + patient.AgeInYears * 3);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.HorseDentalCheck,
            severity,
            "Annual dental examination due",
            $"{patient.Name} has no dental examination recorded in the past 12 months. " +
            "Horses require annual dental floating to prevent sharp points, hooks, and eating difficulties.",
            "Schedule equine dental examination. Assess for sharp enamel points, hooks, ramps, and wave mouth. Float as needed under sedation.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static int MonthsSince(DateTime date)
    {
        var now = DateTime.UtcNow;
        return (now.Year - date.Year) * 12 + (now.Month - date.Month);
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
