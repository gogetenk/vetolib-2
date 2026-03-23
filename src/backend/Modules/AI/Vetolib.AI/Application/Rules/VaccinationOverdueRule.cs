using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// WSAVA: Core vaccinations should be kept up to date.
/// Flags patients with no vaccination record keyword in recent records.
/// </summary>
internal class VaccinationOverdueRule : IHealthAlertRule
{
    public string RuleId => "VACCINATION_OVERDUE";

    private static readonly string[] VaccineKeywords = new[]
    {
        "vaccine", "vaccination", "vaccinated", "booster", "rabies", "dhpp", "fvrcp", "distemper"
    };

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        // Only applies to dogs and cats (core vaccines)
        if (patient.Species is not (MedicalRecords.Contracts.Species.Dog or MedicalRecords.Contracts.Species.Cat))
            return [];

        if (patient.AgeInMonths < 4)
            return []; // Too young, puppy/kitten series managed separately

        if (IsDuplicate(existingAlerts))
            return [];

        var hasRecentVaccination = VaccineKeywords.Any(kw =>
            patient.HasDiagnosisContaining(kw));

        if (hasRecentVaccination)
            return [];

        var severity = patient.AgeInYears >= 1
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 60 + patient.AgeInYears * 3);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.VaccineGap,
            severity,
            "Vaccination may be overdue",
            $"{patient.Name} has no recent vaccination records. Core vaccinations should be verified and updated per WSAVA guidelines.",
            "Review vaccination history and schedule boosters if needed.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
