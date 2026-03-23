using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Patients with prior arthritis/osteoarthritis diagnosis should have
/// regular follow-up (every 3-6 months) for pain management and mobility assessment.
/// </summary>
internal class ArthritisFollowUpRule : IHealthAlertRule
{
    public string RuleId => "ARTHRITIS_FOLLOWUP";

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (!patient.HasDiagnosisContaining("arthritis") && !patient.HasDiagnosisContaining("osteoarthritis") &&
            !patient.HasDiagnosisContaining("DJD") && !patient.HasDiagnosisContaining("degenerative joint"))
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var severity = patient.AgeInYears >= 10
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 50 + patient.AgeInYears * 3);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.ChronicDiseaseFollowUp,
            severity,
            "Arthritis follow-up due",
            $"{patient.Name} has a history of arthritis/DJD. Regular follow-up every 3-6 months is recommended for pain management and mobility assessment.",
            "Schedule arthritis follow-up with pain assessment and mobility evaluation.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
