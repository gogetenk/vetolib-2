using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.UpdateMedicalRecordTemplate;

internal class UpdateMedicalRecordTemplateHandler : IRequestHandler<UpdateMedicalRecordTemplateCommand, Result<MedicalRecordTemplateDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public UpdateMedicalRecordTemplateHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MedicalRecordTemplateDto>> Handle(UpdateMedicalRecordTemplateCommand cmd, CancellationToken ct)
    {
        var template = await _context.MedicalRecordTemplates
            .FirstOrDefaultAsync(t => t.Id == cmd.TemplateId, ct);

        if (template is null)
            return Result<MedicalRecordTemplateDto>.NotFound("Template not found");

        var updateResult = template.Update(
            cmd.Name,
            cmd.Category,
            cmd.DiagnosisTemplate,
            cmd.TreatmentTemplate,
            cmd.NotesTemplate,
            cmd.Species,
            cmd.SortOrder);

        if (!updateResult.IsSuccess)
        {
            if (updateResult.Status == Ardalis.Result.ResultStatus.Invalid)
                return Result<MedicalRecordTemplateDto>.Invalid(updateResult.ValidationErrors.ToList());

            return Result<MedicalRecordTemplateDto>.Error(new ErrorList(updateResult.Errors.ToArray()));
        }

        await _context.SaveChangesAsync(ct);

        return Result<MedicalRecordTemplateDto>.Success(template.ToDto());
    }
}
