using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.DeleteMedicalRecordTemplate;

internal class DeleteMedicalRecordTemplateHandler : IRequestHandler<DeleteMedicalRecordTemplateCommand, Result>
{
    private readonly MedicalRecordsDbContext _context;

    public DeleteMedicalRecordTemplateHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteMedicalRecordTemplateCommand cmd, CancellationToken ct)
    {
        var template = await _context.MedicalRecordTemplates
            .FirstOrDefaultAsync(t => t.Id == cmd.TemplateId, ct);

        if (template is null)
            return Result.NotFound("Template not found");

        var deleteResult = template.Delete();
        if (!deleteResult.IsSuccess)
            return Result.Error(new ErrorList(deleteResult.Errors.ToArray()));

        _context.MedicalRecordTemplates.Remove(template);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
