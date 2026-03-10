using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetActivePrescriptionsForPatient;

internal class GetActivePrescriptionsForPatientHandler
    : IRequestHandler<GetActivePrescriptionsForPatientQuery, Result<List<PrescriptionDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetActivePrescriptionsForPatientHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PrescriptionDto>>> Handle(
        GetActivePrescriptionsForPatientQuery request,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-request.ActiveWindowDays);

        // Prescriptions are children of MedicalRecord, which has a PatientId.
        // The multi-tenant filter applies automatically via MedicalRecords DbContext.
        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Where(p => p.CreatedAt >= cutoff)
            .Join(
                _context.MedicalRecords.Where(mr => mr.PatientId == request.PatientId),
                prescription => prescription.MedicalRecordId,
                medicalRecord => medicalRecord.Id,
                (prescription, _) => prescription)
            .ToListAsync(cancellationToken);

        var dtos = prescriptions.Select(p => p.ToDto()).ToList();

        return Result<List<PrescriptionDto>>.Success(dtos);
    }
}
