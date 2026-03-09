using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class MedicalRecord : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public string Diagnosis { get; private set; } = string.Empty;
    public string Treatment { get; private set; } = string.Empty;
    public string VetName { get; private set; } = string.Empty;
    public DateTime ExaminedAt { get; private set; }

    private readonly List<Prescription> _prescriptions = [];
    public IReadOnlyList<Prescription> Prescriptions => _prescriptions.AsReadOnly();

    private MedicalRecord() { } // EF Core

    public static Result<MedicalRecord> Create(
        Guid clinicId,
        Guid patientId,
        string diagnosis,
        string treatment,
        string vetName,
        DateTime examinedAt)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId est requis"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId est requis"));

        if (string.IsNullOrWhiteSpace(diagnosis))
            errors.Add(new ValidationError(nameof(diagnosis), "Le diagnostic est requis"));

        if (string.IsNullOrWhiteSpace(treatment))
            errors.Add(new ValidationError(nameof(treatment), "Le traitement est requis"));

        if (string.IsNullOrWhiteSpace(vetName))
            errors.Add(new ValidationError(nameof(vetName), "Le nom du vétérinaire est requis"));

        if (errors.Count > 0)
            return Result<MedicalRecord>.Invalid(errors);

        var record = new MedicalRecord
        {
            ClinicId = clinicId,
            PatientId = patientId,
            Diagnosis = diagnosis.Trim(),
            Treatment = treatment.Trim(),
            VetName = vetName.Trim(),
            ExaminedAt = examinedAt
        };

        return Result<MedicalRecord>.Success(record);
    }

    public void AddPrescription(Prescription prescription)
    {
        _prescriptions.Add(prescription);
    }

    public MedicalRecordDto ToDto()
    {
        return new MedicalRecordDto(
            Id,
            PatientId,
            ClinicId,
            Diagnosis,
            Treatment,
            VetName,
            ExaminedAt,
            _prescriptions.Select(p => p.ToDto()).ToList());
    }
}
