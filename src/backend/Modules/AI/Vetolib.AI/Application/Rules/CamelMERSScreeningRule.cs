using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// MERS-CoV (Middle East Respiratory Syndrome) is a zoonotic concern in
/// dromedary camels in the UAE. Respiratory symptoms in camels should
/// trigger MERS screening due to public health implications.
/// </summary>
internal class CamelMERSScreeningRule : IHealthAlertRule
{
    public string RuleId => "CAMEL_MERS_SCREENING";

    private static readonly string[] RespiratoryKeywords =
    [
        "respiratory", "nasal discharge", "cough", "dyspnea",
        "pneumonia", "lung", "breathing difficulty", "wheezing",
        "sneezing", "tracheal"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Camel)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Already tested/cleared
        if (patient.HasDiagnosisContaining("MERS negative") || patient.HasDiagnosisContaining("MERS-CoV cleared"))
            return [];

        var hasRespiratorySign = RespiratoryKeywords.Any(k => patient.HasDiagnosisContaining(k));

        if (!hasRespiratorySign)
            return [];

        // MERS is always high severity due to zoonotic public health risk
        var riskScore = 85;
        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.CamelMERSScreening,
            HealthAlertSeverity.High,
            "MERS-CoV screening recommended — public health concern",
            $"{patient.Name} presents with respiratory symptoms. Dromedary camels are known reservoirs of MERS-CoV. Screening is recommended for public health safety.",
            "Collect nasal swab for RT-PCR MERS-CoV testing. Isolate animal pending results. Notify handlers of zoonotic precautions. Report positive results to authorities.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
