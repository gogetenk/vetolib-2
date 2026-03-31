using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Surra (Trypanosoma evansi) is the most significant protozoan disease of
/// camels in the UAE, transmitted by biting flies. Weight loss combined with
/// anemia signs (pale mucous membranes, lethargy) should trigger screening.
/// </summary>
internal class CamelTrypanosomaRule : IHealthAlertRule
{
    public string RuleId => "CAMEL_TRYPANOSOMA";

    private static readonly string[] AnemiaKeywords =
    [
        "anemia", "anaemia", "pale mucous", "pale membrane",
        "lethargy", "lethargic", "icterus", "jaundice", "edema"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Camel)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Already diagnosed and under treatment
        if (patient.HasDiagnosisContaining("trypanosoma") || patient.HasDiagnosisContaining("surra treatment"))
            return [];

        var hasAnemiaSign = AnemiaKeywords.Any(k => patient.HasDiagnosisContaining(k));
        var hasWeightLoss = HasRecentWeightLoss(patient);

        // Both weight loss AND anemia signs required
        if (!hasAnemiaSign || !hasWeightLoss)
            return [];

        var riskScore = 75;
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CamelTrypanosoma,
            HealthAlertSeverity.High,
            "Surra (Trypanosoma evansi) screening recommended",
            $"{patient.Name} presents with weight loss and anemia signs. Surra is endemic in UAE camels and can be fatal if untreated.",
            "Request blood smear and CATT/T. evansi serological test. Consider prophylactic suramin treatment in endemic areas.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private static bool HasRecentWeightLoss(PatientAlertContext patient)
    {
        if (patient.WeightHistory.Count < 2)
            return false;

        var sorted = patient.WeightHistory.OrderByDescending(w => w.RecordedAt).ToList();
        var latest = sorted[0];
        var previous = sorted[1];

        if (previous.WeightKg <= 0)
            return false;

        var changePercent = ((latest.WeightKg - previous.WeightKg) / previous.WeightKg) * 100m;
        return changePercent < -5m;
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
