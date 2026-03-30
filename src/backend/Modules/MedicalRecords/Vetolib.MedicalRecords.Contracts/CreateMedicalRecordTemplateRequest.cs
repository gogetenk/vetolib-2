namespace Vetolib.MedicalRecords.Contracts;

public record CreateMedicalRecordTemplateRequest(
    string Name,
    TemplateCategory Category,
    string DiagnosisTemplate,
    string TreatmentTemplate,
    string NotesTemplate,
    Species? Species,
    int SortOrder = 0);
