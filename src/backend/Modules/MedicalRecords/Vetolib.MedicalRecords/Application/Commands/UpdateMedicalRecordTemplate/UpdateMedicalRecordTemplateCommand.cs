using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.UpdateMedicalRecordTemplate;

internal record UpdateMedicalRecordTemplateCommand(
    Guid TemplateId,
    string Name,
    TemplateCategory Category,
    string DiagnosisTemplate,
    string TreatmentTemplate,
    string NotesTemplate,
    Species? Species,
    int SortOrder) : IRequest<Result<MedicalRecordTemplateDto>>;
