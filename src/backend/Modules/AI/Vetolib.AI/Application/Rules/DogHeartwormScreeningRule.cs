using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AHS (American Heartworm Society): Dogs should be tested for heartworm annually.
/// No heartworm test in 12+ months triggers this alert.
/// </summary>
internal class DogHeartwormScreeningRule : IHealthAlertRule
{
    public string RuleId => "DOG_HEARTWORM_SCREENING";

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Dog)
            return [];

        if (patient.AgeInMonths < 6)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("heartworm"))
            return [];

        var severity = patient.AgeInYears >= 5
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 40 + patient.AgeInYears * 5);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.DogHeartwormScreening,
            severity,
            "Annual heartworm test due",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {patient.Breed} dog with no recent heartworm test on file. Annual testing is recommended per AHS guidelines.",
            "Schedule heartworm antigen test (4Dx SNAP or equivalent).",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
