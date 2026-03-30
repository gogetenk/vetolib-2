using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;

internal class UploadPatientPhotoHandler : IRequestHandler<UploadPatientPhotoCommand, Result>
{
    private readonly MedicalRecordsDbContext _context;

    public UploadPatientPhotoHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UploadPatientPhotoCommand cmd, CancellationToken ct)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == cmd.PatientId, ct);

        if (patient is null)
            return Result.NotFound($"Patient '{cmd.PatientId}' not found.");

        var result = patient.SetPhoto(cmd.PhotoData, cmd.ContentType);
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
