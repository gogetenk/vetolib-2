using Ardalis.Result;

namespace Vetolib.MedicalRecords.Contracts;

public interface IPatientAlertDataReader
{
    Task<Result<IReadOnlyList<PatientAlertDataDto>>> GetAllActivePatientsWithRecordsAsync(CancellationToken ct = default);
}

public record PatientAlertDataDto(
    Guid PatientId,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    decimal? WeightKg,
    IReadOnlyList<MedicalRecordSummaryDto> RecentRecords,
    IReadOnlyList<WeightEntryDto> WeightHistory);

public record WeightEntryDto(
    Guid Id,
    Guid PatientId,
    decimal WeightKg,
    DateTime RecordedAt,
    string RecordedBy,
    string? Note);
