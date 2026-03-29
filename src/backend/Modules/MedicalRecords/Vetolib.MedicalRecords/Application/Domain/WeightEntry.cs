using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class WeightEntry : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public decimal WeightKg { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public string RecordedBy { get; private set; } = string.Empty;
    public string? Note { get; private set; }

    private WeightEntry() { } // EF Core constructor

    public static Result<WeightEntry> Create(
        Guid clinicId,
        Guid patientId,
        decimal weightKg,
        string recordedBy,
        string? note = null,
        DateTime? recordedAt = null)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId is required"));

        if (weightKg <= 0)
            errors.Add(new ValidationError(nameof(weightKg), "Weight must be greater than zero"));

        if (weightKg > 10000)
            errors.Add(new ValidationError(nameof(weightKg), "Weight exceeds maximum allowed value"));

        if (string.IsNullOrWhiteSpace(recordedBy))
            errors.Add(new ValidationError(nameof(recordedBy), "RecordedBy is required"));

        var trimmedNote = note?.Trim();
        if (trimmedNote is not null && trimmedNote.Length > 500)
            trimmedNote = trimmedNote[..500];

        if (errors.Count > 0)
            return Result<WeightEntry>.Invalid(errors);

        return Result<WeightEntry>.Success(new WeightEntry
        {
            ClinicId = clinicId,
            PatientId = patientId,
            WeightKg = weightKg,
            RecordedBy = recordedBy.Trim(),
            Note = string.IsNullOrWhiteSpace(trimmedNote) ? null : trimmedNote,
            RecordedAt = recordedAt ?? DateTime.UtcNow
        });
    }

    public WeightEntryDto ToDto() => new(
        Id,
        PatientId,
        WeightKg,
        RecordedAt,
        RecordedBy,
        Note);

    public WeightCurvePointDto ToCurvePointDto() => new(
        RecordedAt,
        WeightKg);
}
