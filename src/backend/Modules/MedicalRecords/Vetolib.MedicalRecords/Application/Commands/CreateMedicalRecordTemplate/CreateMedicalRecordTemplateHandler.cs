using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.CreateMedicalRecordTemplate;

internal class CreateMedicalRecordTemplateHandler : IRequestHandler<CreateMedicalRecordTemplateCommand, Result<MedicalRecordTemplateDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public CreateMedicalRecordTemplateHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MedicalRecordTemplateDto>> Handle(CreateMedicalRecordTemplateCommand cmd, CancellationToken ct)
    {
        var templateResult = MedicalRecordTemplate.Create(
            cmd.ClinicId,
            cmd.Name,
            cmd.Category,
            cmd.DiagnosisTemplate,
            cmd.TreatmentTemplate,
            cmd.NotesTemplate,
            cmd.Species,
            isSystemTemplate: false,
            cmd.SortOrder);

        if (!templateResult.IsSuccess)
            return Result<MedicalRecordTemplateDto>.Invalid(templateResult.ValidationErrors.ToList());

        _context.MedicalRecordTemplates.Add(templateResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<MedicalRecordTemplateDto>.Success(templateResult.Value.ToDto());
    }
}
