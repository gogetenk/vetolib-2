using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class MedicalRecordTemplate : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TemplateCategory Category { get; private set; }
    public string DiagnosisTemplate { get; private set; } = string.Empty;
    public string TreatmentTemplate { get; private set; } = string.Empty;
    public string NotesTemplate { get; private set; } = string.Empty;
    public Species? Species { get; private set; }
    public bool IsSystemTemplate { get; private set; }
    public int SortOrder { get; private set; }

    private MedicalRecordTemplate() { } // EF Core

    public static Result<MedicalRecordTemplate> Create(
        Guid clinicId,
        string name,
        TemplateCategory category,
        string diagnosisTemplate,
        string treatmentTemplate,
        string notesTemplate,
        Species? species,
        bool isSystemTemplate,
        int sortOrder)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Template name is required"));

        if (name is not null && name.Length > 200)
            errors.Add(new ValidationError(nameof(name), "Template name cannot exceed 200 characters"));

        if (diagnosisTemplate is not null && diagnosisTemplate.Length > 2000)
            errors.Add(new ValidationError(nameof(diagnosisTemplate), "Diagnosis template cannot exceed 2000 characters"));

        if (treatmentTemplate is not null && treatmentTemplate.Length > 2000)
            errors.Add(new ValidationError(nameof(treatmentTemplate), "Treatment template cannot exceed 2000 characters"));

        if (notesTemplate is not null && notesTemplate.Length > 2000)
            errors.Add(new ValidationError(nameof(notesTemplate), "Notes template cannot exceed 2000 characters"));

        if (errors.Count > 0)
            return Result<MedicalRecordTemplate>.Invalid(errors);

        var template = new MedicalRecordTemplate
        {
            ClinicId = clinicId,
            Name = name!.Trim(),
            Category = category,
            DiagnosisTemplate = diagnosisTemplate?.Trim() ?? string.Empty,
            TreatmentTemplate = treatmentTemplate?.Trim() ?? string.Empty,
            NotesTemplate = notesTemplate?.Trim() ?? string.Empty,
            Species = species,
            IsSystemTemplate = isSystemTemplate,
            SortOrder = sortOrder
        };

        return Result<MedicalRecordTemplate>.Success(template);
    }

    public Result Update(
        string name,
        TemplateCategory category,
        string diagnosisTemplate,
        string treatmentTemplate,
        string notesTemplate,
        Species? species,
        int sortOrder)
    {
        if (IsSystemTemplate)
            return Result.Error("SYSTEM_TEMPLATE_READONLY:System templates cannot be modified");

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Template name is required"));

        if (name is not null && name.Length > 200)
            errors.Add(new ValidationError(nameof(name), "Template name cannot exceed 200 characters"));

        if (diagnosisTemplate is not null && diagnosisTemplate.Length > 2000)
            errors.Add(new ValidationError(nameof(diagnosisTemplate), "Diagnosis template cannot exceed 2000 characters"));

        if (treatmentTemplate is not null && treatmentTemplate.Length > 2000)
            errors.Add(new ValidationError(nameof(treatmentTemplate), "Treatment template cannot exceed 2000 characters"));

        if (notesTemplate is not null && notesTemplate.Length > 2000)
            errors.Add(new ValidationError(nameof(notesTemplate), "Notes template cannot exceed 2000 characters"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name!.Trim();
        Category = category;
        DiagnosisTemplate = diagnosisTemplate?.Trim() ?? string.Empty;
        TreatmentTemplate = treatmentTemplate?.Trim() ?? string.Empty;
        NotesTemplate = notesTemplate?.Trim() ?? string.Empty;
        Species = species;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Delete()
    {
        if (IsSystemTemplate)
            return Result.Error("SYSTEM_TEMPLATE_READONLY:System templates cannot be deleted");

        return Result.Success();
    }

    public MedicalRecordTemplateDto ToDto()
    {
        return new MedicalRecordTemplateDto(
            Id,
            Name,
            Category,
            DiagnosisTemplate,
            TreatmentTemplate,
            NotesTemplate,
            Species,
            IsSystemTemplate,
            SortOrder);
    }
}
