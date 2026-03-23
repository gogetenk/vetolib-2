using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

internal record PatientAlertContext(
    Guid ClinicId,
    Guid PatientId,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    decimal? WeightKg,
    IReadOnlyList<MedicalRecordSummaryDto> RecentRecords,
    IReadOnlyList<WeightEntryDto> WeightHistory)
{
    public int AgeInMonths
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return (today.Year - BirthDate.Year) * 12 + (today.Month - BirthDate.Month);
        }
    }

    public int AgeInYears => AgeInMonths / 12;

    public bool HasDiagnosisContaining(string keyword)
        => RecentRecords.Any(r => r.Diagnosis.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
