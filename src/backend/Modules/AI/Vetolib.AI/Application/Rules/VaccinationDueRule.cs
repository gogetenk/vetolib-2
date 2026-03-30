using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// UAE requires annual rabies vaccination. WSAVA recommends core vaccines on schedule.
/// Triggers when the most recent vaccination record is older than 11 months,
/// giving 1 month lead time before the annual due date.
/// Only applies to dogs and cats (falcons have different vaccination schedules).
/// </summary>
internal class VaccinationDueRule : IHealthAlertRule
{
    public string RuleId => "VACCINATION_DUE";

    private const int ReminderThresholdMonths = 11;
    private const int MinAgeMonths = 4;

    private static readonly string[] VaccineKeywords =
    [
        "vaccine", "vaccination", "vaccinated", "booster", "rabies", "dhpp", "fvrcp", "distemper"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species is not (Species.Dog or Species.Cat))
            return [];

        if (patient.AgeInMonths < MinAgeMonths)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var vaccinationRecords = patient.RecentRecords
            .Where(r => VaccineKeywords.Any(kw =>
                r.Diagnosis.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // No vaccination records at all => handled by VaccinationOverdueRule
        if (vaccinationRecords.Count == 0)
            return [];

        var mostRecentVaccination = vaccinationRecords.Max(r => r.ExaminedAt);
        var monthsSinceLastVaccination = MonthsSince(mostRecentVaccination);

        if (monthsSinceLastVaccination < ReminderThresholdMonths)
            return [];

        var riskScore = Math.Min(100, 40 + monthsSinceLastVaccination * 3);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.VaccinationDue,
            HealthAlertSeverity.Medium,
            "Vaccination due soon",
            $"{patient.Name}'s last vaccination was {monthsSinceLastVaccination} months ago. " +
            "Annual vaccination boosters should be scheduled to maintain compliance with UAE regulations.",
            "Schedule vaccination appointment within the next month.",
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
