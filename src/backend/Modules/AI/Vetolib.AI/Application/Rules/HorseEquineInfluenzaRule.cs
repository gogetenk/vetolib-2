using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Equine influenza is a highly contagious respiratory disease.
/// Combination of respiratory symptoms (cough, nasal discharge) with fever
/// should trigger flu screening and isolation recommendation.
/// </summary>
internal class HorseEquineInfluenzaRule : IHealthAlertRule
{
    public string RuleId => "HORSE_EQUINE_INFLUENZA";

    private static readonly string[] RespiratoryKeywords =
    [
        "cough", "nasal discharge", "respiratory", "dyspnea", "wheezing",
        "serous discharge", "mucopurulent", "tachypnea"
    ];

    private static readonly string[] FeverKeywords =
    [
        "fever", "pyrexia", "hyperthermia", "temperature elevated", "38.5"
    ];

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Horse)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        var hasRespiratorySign = RespiratoryKeywords.Any(k => patient.HasDiagnosisContaining(k));
        var hasFever = FeverKeywords.Any(k => patient.HasDiagnosisContaining(k));

        // Need both respiratory signs AND fever indicators
        if (!hasRespiratorySign || !hasFever)
            return [];

        // Already diagnosed or vaccinated recently
        if (patient.HasDiagnosisContaining("influenza confirmed") ||
            patient.HasDiagnosisContaining("influenza vaccine"))
            return [];

        var riskScore = 70;

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.HorseEquineInfluenza,
            HealthAlertSeverity.High,
            "Equine influenza screening recommended",
            $"{patient.Name} presents with respiratory symptoms and fever, consistent with equine influenza. " +
            "EI is highly contagious and can spread rapidly through stables.",
            "Isolate from other horses immediately. Collect nasopharyngeal swab for PCR testing. " +
            "Provide supportive care (rest, NSAIDs for fever). Notify barn management of potential outbreak.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
