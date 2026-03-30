using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// In the UAE, falcons require annual health certificates for legal hunting
/// and transport. If no health certificate record is found in the past 12 months,
/// a reminder alert is generated.
/// </summary>
internal class FalconHealthCertificateRule : IHealthAlertRule
{
    public string RuleId => "FALCON_HEALTH_CERTIFICATE";

    private static readonly string[] CertificateKeywords =
    [
        "health certificate", "fitness certificate", "annual certificate",
        "export certificate", "CITES", "hunting permit"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Falcon)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Falcon must be at least 6 months old
        if (patient.AgeInMonths < 6)
            return [];

        var hasCertificate = CertificateKeywords.Any(k => patient.HasDiagnosisContaining(k));

        if (hasCertificate)
            return [];

        // Determine severity based on proximity to hunting season (Oct-Mar in UAE)
        var now = DateTime.UtcNow;
        var isHuntingSeason = now.Month >= 10 || now.Month <= 3;
        var severity = isHuntingSeason
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = isHuntingSeason ? 60 : 40;

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.FalconHealthCertificate,
            severity,
            "Annual health certificate missing",
            $"{patient.Name} has no recent health certificate on record. Falcons require annual health certificates for hunting permits and transport in the UAE.",
            "Schedule comprehensive health examination and issue health certificate.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
