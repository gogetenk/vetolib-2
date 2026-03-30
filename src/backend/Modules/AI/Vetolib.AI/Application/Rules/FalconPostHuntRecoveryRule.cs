using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// After the hunting season (Oct-Mar in UAE), falcons need post-season health
/// evaluation. Injuries, parasites, and condition loss accumulated during hunting
/// should be assessed before the off-season.
/// </summary>
internal class FalconPostHuntRecoveryRule : IHealthAlertRule
{
    public string RuleId => "FALCON_POST_HUNT_RECOVERY";

    private static readonly string[] PostHuntKeywords =
    [
        "post-hunt", "post hunt", "hunting injury", "talon injury",
        "prey injury", "flight injury"
    ];

    private static readonly string[] RecoveryKeywords =
    [
        "recovery check", "post-season exam", "hunting season exam"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Post-hunt monitoring window: April-May (right after hunting season ends in March)
        var now = DateTime.UtcNow;
        if (now.Month < 4 || now.Month > 5)
            return [];

        // Skip if a recent recovery exam was done
        var hasRecoveryExam = RecoveryKeywords.Any(k => patient.HasDiagnosisContaining(k));
        if (hasRecoveryExam)
            return [];

        // Higher priority if there are hunting-related injuries in records
        var hasHuntingInjury = PostHuntKeywords.Any(k => patient.HasDiagnosisContaining(k));

        var severity = hasHuntingInjury
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = hasHuntingInjury ? 65 : 45;

        var description = hasHuntingInjury
            ? $"{patient.Name} has hunting-related injuries on record and needs a post-season recovery assessment."
            : $"{patient.Name} should have a post-hunting-season health evaluation. Assess for parasites, injuries, and overall condition before the off-season.";

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconPostHuntRecovery,
            severity,
            "Post-hunting season recovery check due",
            description,
            "Schedule comprehensive post-season examination including parasite screening, weight assessment, and feather condition evaluation.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
