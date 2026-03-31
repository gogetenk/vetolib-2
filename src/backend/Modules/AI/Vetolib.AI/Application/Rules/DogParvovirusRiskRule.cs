using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA Canine Vaccination Guidelines: Puppies under 16 weeks are at high risk for
/// parvovirus if their vaccination series is incomplete. Parvo has 91% mortality if untreated.
/// </summary>
internal class DogParvovirusRiskRule : IHealthAlertRule
{
    public string RuleId => "DOG_PARVOVIRUS_RISK";

    private const int MaxAgeWeeks = 16;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Dog)
            return [];

        // Only applies to puppies under 16 weeks
        if (patient.AgeInMonths >= 4)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        // Check if vaccination series is complete
        if (patient.HasDiagnosisContaining("parvovirus vaccine") ||
            patient.HasDiagnosisContaining("DHPP") ||
            patient.HasDiagnosisContaining("parvo vaccination complete"))
            return [];

        var riskScore = 85; // High baseline risk for unvaccinated puppies

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.DogParvovirusRisk,
            HealthAlertSeverity.High,
            "Parvovirus vaccination incomplete",
            $"{patient.Name} is a {patient.AgeInMonths}-month-old puppy with no complete parvovirus vaccination on file. Canine parvovirus has up to 91% mortality in unvaccinated puppies.",
            "Schedule DHPP vaccination series immediately. Advise owner to limit exposure to unvaccinated dogs and contaminated environments.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
