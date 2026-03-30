using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Falcons commonly lose weight during molting season (August-October).
/// Weight loss >10% during this period warrants veterinary attention
/// as it may indicate illness masked by expected molt-related changes.
/// </summary>
internal class FalconMoltWeightLossRule : IHealthAlertRule
{
    public string RuleId => "FALCON_MOLT_WEIGHT_LOSS";

    private const decimal MoltWeightLossThreshold = 10m;
    private const int MoltStartMonth = 8; // August
    private const int MoltEndMonth = 10;  // October

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var now = DateTime.UtcNow;
        if (now.Month < MoltStartMonth || now.Month > MoltEndMonth)
            return [];

        if (patient.WeightHistory.Count < 2)
            return [];

        var sortedWeights = patient.WeightHistory
            .OrderByDescending(w => w.RecordedAt)
            .ToList();

        var latest = sortedWeights[0];
        // Compare to a pre-molt baseline (any entry before August)
        var preMoltEntry = sortedWeights
            .FirstOrDefault(w => w.RecordedAt.Month < MoltStartMonth && w.RecordedAt.Year == now.Year)
            ?? sortedWeights.FirstOrDefault(w => w.RecordedAt < now.AddMonths(-2));

        if (preMoltEntry is null)
            return [];

        var lossPercent = ((preMoltEntry.WeightKg - latest.WeightKg) / preMoltEntry.WeightKg) * 100m;

        if (lossPercent <= MoltWeightLossThreshold)
            return [];

        var severity = lossPercent >= 20m
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, (int)(40 + lossPercent * 2));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconMoltWeightLoss,
            severity,
            "Excessive weight loss during molting season",
            $"{patient.Name} has lost {lossPercent:F1}% body weight during molting season ({preMoltEntry.WeightKg:F2} kg to {latest.WeightKg:F2} kg). Weight loss >10% during molt (Aug-Oct) may indicate underlying illness.",
            "Schedule weight assessment and full physical examination. Consider CBC and biochemistry panel.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
