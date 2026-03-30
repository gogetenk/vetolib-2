namespace Vetolib.MedicalRecords.Contracts;

public record UpdateMedicalRecordTemplateRequest(
    string Name,
    TemplateCategory Category,
    string DiagnosisTemplate,
    string TreatmentTemplate,
    string NotesTemplate,
    Species? Species,
    int SortOrder = 0);
