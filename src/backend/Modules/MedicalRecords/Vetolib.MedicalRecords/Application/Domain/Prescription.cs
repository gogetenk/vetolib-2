using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class Prescription : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid MedicalRecordId { get; private set; }
    public string Medication { get; private set; } = string.Empty;
    public string Dosage { get; private set; } = string.Empty;
    public string VetLicenseNumber { get; private set; } = string.Empty;
    public Guid? DrugCatalogEntryId { get; private set; }
    public string? OverrideJustification { get; private set; }
    public string? OverrideSeverity { get; private set; }

    private Prescription() { } // EF Core

    public static Result<Prescription> Create(
        Guid clinicId,
        Guid medicalRecordId,
        string medication,
        string dosage,
        string vetLicenseNumber,
        Guid? drugCatalogEntryId = null)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (medicalRecordId == Guid.Empty)
            errors.Add(new ValidationError(nameof(medicalRecordId), "MedicalRecordId is required"));

        if (string.IsNullOrWhiteSpace(medication))
            errors.Add(new ValidationError(nameof(medication), "Medication is required"));

        if (string.IsNullOrWhiteSpace(dosage))
            errors.Add(new ValidationError(nameof(dosage), "Dosage is required"));

        if (string.IsNullOrWhiteSpace(vetLicenseNumber))
            errors.Add(new ValidationError(nameof(vetLicenseNumber), "License number is required"));

        if (errors.Count > 0)
            return Result<Prescription>.Invalid(errors);

        var prescription = new Prescription
        {
            ClinicId = clinicId,
            MedicalRecordId = medicalRecordId,
            Medication = medication.Trim(),
            Dosage = dosage.Trim(),
            VetLicenseNumber = vetLicenseNumber.Trim(),
            DrugCatalogEntryId = drugCatalogEntryId
        };

        return Result<Prescription>.Success(prescription);
    }

    public Result ApplyOverride(string justification, InteractionSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(justification) || justification.Trim().Length < 10)
            return Result.Error("Override justification must be at least 10 characters");

        OverrideJustification = justification.Trim();
        OverrideSeverity = severity.ToString();
        return Result.Success();
    }

    public PrescriptionDto ToDto()
    {
        return new PrescriptionDto(
            Id,
            MedicalRecordId,
            ClinicId,
            Medication,
            Dosage,
            VetLicenseNumber,
            CreatedAt,
            DrugCatalogEntryId,
            OverrideJustification,
            OverrideSeverity);
    }
}
