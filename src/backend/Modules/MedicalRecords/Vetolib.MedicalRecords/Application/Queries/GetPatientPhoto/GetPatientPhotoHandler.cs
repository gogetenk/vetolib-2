using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientPhoto;

internal class GetPatientPhotoHandler : IRequestHandler<GetPatientPhotoQuery, Result<PatientPhotoResult>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientPhotoHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientPhotoResult>> Handle(GetPatientPhotoQuery query, CancellationToken ct)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PatientId, ct);

        if (patient is null)
            return Result<PatientPhotoResult>.NotFound("Patient not found.");

        var photoResult = patient.GetPhoto();
        if (!photoResult.IsSuccess)
            return Result<PatientPhotoResult>.NotFound("Patient has no photo.");

        var (data, contentType) = photoResult.Value;
        return Result<PatientPhotoResult>.Success(new PatientPhotoResult(data, contentType));
    }
}
