namespace Vetolib.MedicalRecords.Contracts;

public record MedicalRecordTemplateDto(
    Guid Id,
    string Name,
    TemplateCategory Category,
    string DiagnosisTemplate,
    string TreatmentTemplate,
    string NotesTemplate,
    Species? Species,
    bool IsSystemTemplate,
    int SortOrder);
