using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// Large breed dogs predisposed to hip dysplasia should be screened
/// from 1-2 years of age (OFA/PennHIP recommendations).
/// </summary>
internal class HipDysplasiaRule : IHealthAlertRule
{
    public string RuleId => "HIP_DYSPLASIA_SCREENING";

    private const int MinAgeYears = 1;

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Dog)
            return [];

        if (!BreedRiskData.IsBreedInList(patient.Breed, BreedRiskData.HipDysplasiaBreeds))
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("hip") || patient.HasDiagnosisContaining("dysplasia") || patient.HasDiagnosisContaining("OFA") || patient.HasDiagnosisContaining("PennHIP"))
            return [];

        var severity = patient.AgeInYears >= 5
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 40 + patient.AgeInYears * 5);

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.BreedSpecificScreening,
            severity,
            "Hip dysplasia screening recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {patient.Breed} at elevated risk for hip dysplasia. Radiographic screening is recommended per OFA guidelines.",
            "Schedule hip radiographs for OFA or PennHIP evaluation.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
