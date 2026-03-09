using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientDetail;

internal class GetPatientDetailHandler : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientDetailHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientDetailDto>> Handle(GetPatientDetailQuery query, CancellationToken ct)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == query.PatientId, ct);

        if (patient is null)
            return Result<PatientDetailDto>.NotFound($"Patient '{query.PatientId}' not found.");

        // Load last 5 medical records
        var recentRecords = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId)
            .OrderByDescending(r => r.ExaminedAt)
            .Take(5)
            .Select(r => new MedicalRecordSummaryDto(r.Id, r.Diagnosis, r.VetName, r.ExaminedAt))
            .ToListAsync(ct);

        var detail = new PatientDetailDto(patient.ToDto(), recentRecords);
        return Result<PatientDetailDto>.Success(detail);
    }
}
