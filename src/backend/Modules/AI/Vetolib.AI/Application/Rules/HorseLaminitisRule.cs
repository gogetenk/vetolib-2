using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Laminitis is a painful and potentially fatal hoof condition. Overweight horses
/// are at higher risk, especially during spring when lush grass causes sugar overload.
/// Arabian horses and ponies are particularly susceptible.
/// </summary>
internal class HorseLaminitisRule : IHealthAlertRule
{
    public string RuleId => "HORSE_LAMINITIS";

    // Average horse weight ~500 kg; above 550 kg considered overweight for most breeds
    private const decimal OverweightThresholdKg = 550m;

    // Spring months (March-May) have the highest grass sugar content
    private static readonly int[] SpringMonths = [3, 4, 5];

    private static readonly string[] HighRiskBreeds =
    [
        "arabian", "arab", "pony", "welsh", "shetland", "morgan",
        "quarter horse", "appaloosa", "paso fino"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Horse)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var isOverweight = patient.WeightKg.HasValue && patient.WeightKg.Value > OverweightThresholdKg;
        var isSpring = SpringMonths.Contains(DateTime.UtcNow.Month);
        var isHighRiskBreed = HighRiskBreeds.Any(b =>
            patient.Breed.Contains(b, StringComparison.OrdinalIgnoreCase));

        // Need overweight + spring season OR overweight + high-risk breed
        if (!isOverweight)
            return [];

        if (!isSpring && !isHighRiskBreed)
            return [];

        // Already being managed for laminitis
        if (patient.HasDiagnosisContaining("laminitis") || patient.HasDiagnosisContaining("founder"))
            return [];

        var severity = isSpring && isHighRiskBreed
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = isSpring && isHighRiskBreed ? 75 : 55;

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.HorseLaminitis,
            severity,
            "Laminitis risk — weight and seasonal factors",
            $"{patient.Name} is overweight ({patient.WeightKg:F0} kg) " +
            (isSpring ? "during spring grass season. " : "and belongs to a high-risk breed. ") +
            "Laminitis is a serious hoof condition that can be fatal if untreated.",
            "Restrict pasture access, especially during spring. Evaluate diet and consider grazing muzzle. Schedule farrier visit and metabolic panel (insulin, ACTH).",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
