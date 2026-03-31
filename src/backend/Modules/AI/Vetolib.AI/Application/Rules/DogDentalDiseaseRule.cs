using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

/// <summary>
/// AAHA Dental Care Guidelines: 80% of dogs show signs of periodontal disease by age 3.
/// Dogs over 3 years with no dental cleaning in 12+ months should be flagged.
/// Small breeds are at higher risk.
/// </summary>
internal class DogDentalDiseaseRule : IHealthAlertRule
{
    public string RuleId => "DOG_DENTAL_DISEASE";

    private const int MinAgeYears = 3;

    private static readonly IReadOnlyList<string> SmallBreeds = new[]
    {
        "Yorkshire Terrier", "Chihuahua", "Pomeranian", "Maltese",
        "Dachshund", "Shih Tzu", "Pug", "Cavalier King Charles Spaniel",
        "Miniature Schnauzer", "Toy Poodle", "Miniature Poodle"
    };

    public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
    {
        if (patient.Species != Species.Dog)
            return [];

        if (patient.AgeInYears < MinAgeYears)
            return [];

        if (IsDuplicate(existingAlerts))
            return [];

        if (patient.HasDiagnosisContaining("dental cleaning") ||
            patient.HasDiagnosisContaining("dental prophylaxis") ||
            patient.HasDiagnosisContaining("teeth cleaning"))
            return [];

        var isSmallBreed = BreedRiskData.IsBreedInList(patient.Breed, SmallBreeds);
        var severity = isSmallBreed || patient.AgeInYears >= 7
            ? HealthAlertSeverity.High
            : HealthAlertSeverity.Medium;

        var riskScore = Math.Min(100, 35 + patient.AgeInYears * 5 + (isSmallBreed ? 15 : 0));

        var result = HealthAlert.Create(
            patient.ClinicId,
            patient.PatientId,
            HealthAlertType.DogDentalDisease,
            severity,
            "Dental cleaning recommended",
            $"{patient.Name} is a {patient.AgeInYears}-year-old {patient.Breed} with no recent dental cleaning. 80% of dogs develop periodontal disease by age 3.{(isSmallBreed ? " Small breeds are at higher risk." : "")}",
            "Schedule dental examination and prophylaxis under anesthesia. Consider dental radiographs.",
            RuleId,
            riskScore);

        return result.IsSuccess ? [result.Value] : [];
    }

    private bool IsDuplicate(IReadOnlyList<HealthAlert> existingAlerts)
        => existingAlerts.Any(a => a.RuleId == RuleId && a.Status != HealthAlertStatus.Dismissed);
}
