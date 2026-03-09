using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientById;

internal class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientByIdHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery query, CancellationToken ct)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == query.PatientId, ct);

        if (patient is null)
            return Result<PatientDto>.NotFound("PATIENT_NOT_FOUND");

        return Result<PatientDto>.Success(patient.ToDto());
    }
}
