using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Camels in the UAE are at risk of heat stress during summer months (June-September)
/// when temperatures regularly exceed 45°C. An exam during this period without
/// shade/cooling notes should trigger a heat stress risk alert.
/// </summary>
internal class CamelHeatStressRule : IHealthAlertRule
{
    public string RuleId => "CAMEL_HEAT_STRESS";

    private static readonly string[] ShadeKeywords =
    [
        "shade", "cooling", "air condition", "misting", "water access",
        "shelter", "barn", "indoor", "climate control"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Camel)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Only alert during UAE summer months (June-September)
        var now = DateTime.UtcNow;
        if (now.Month < 6 || now.Month > 9)
            return [];

        // Skip if shade/cooling is documented in recent records
        var hasShadeNotes = ShadeKeywords.Any(k => patient.HasDiagnosisContaining(k));
        if (hasShadeNotes)
            return [];

        var severity = now.Month is 7 or 8
            ? HealthAlertSeverity.High   // Peak summer
            : HealthAlertSeverity.Medium; // Jun/Sep shoulder months

        var riskScore = now.Month is 7 or 8 ? 70 : 50;

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CamelHeatStress,
            severity,
            "Heat stress risk — summer monitoring needed",
            $"{patient.Name} has no shade or cooling management documented during the UAE summer period. Camels are susceptible to heat stress above 42°C.",
            "Verify shade availability, water access, and feeding schedule. Consider electrolyte supplementation. Document environmental management plan.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
