using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;

internal class ExportPatientFhirHandler : IRequestHandler<ExportPatientFhirQuery, Result<FhirBundleResult>>
{
    private readonly MedicalRecordsDbContext _context;

    public ExportPatientFhirHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FhirBundleResult>> Handle(ExportPatientFhirQuery query, CancellationToken ct)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == query.PatientId, ct);

        if (patient is null)
            return Result<FhirBundleResult>.NotFound($"Patient '{query.PatientId}' not found.");

        var owner = patient.PatientOwners
            .FirstOrDefault(po => po.Owner is not null)?.Owner;

        var medicalRecords = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId)
            .OrderByDescending(r => r.ExaminedAt)
            .Take(1000)
            .Include(r => r.Prescriptions)
            .AsSplitQuery()
            .ToListAsync(ct);

        var weightEntries = await _context.WeightEntries
            .AsNoTracking()
            .Where(w => w.PatientId == query.PatientId)
            .OrderByDescending(w => w.RecordedAt)
            .Take(1000)
            .ToListAsync(ct);

        var json = FhirR4Mapper.BuildBundle(patient, owner, medicalRecords, weightEntries);

        return Result<FhirBundleResult>.Success(
            new FhirBundleResult(json, FhirBundleResult.FhirJsonContentType));
    }
}
