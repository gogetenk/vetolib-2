using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientSpeciesAndWeight;

internal class GetPatientSpeciesAndWeightHandler
    : IRequestHandler<GetPatientSpeciesAndWeightQuery, Result<PatientSpeciesWeightDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientSpeciesAndWeightHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientSpeciesWeightDto>> Handle(
        GetPatientSpeciesAndWeightQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == request.PatientId)
            .Select(p => new { p.Id, p.Species, p.WeightKg })
            .FirstOrDefaultAsync(cancellationToken);

        if (patient is null)
            return Result<PatientSpeciesWeightDto>.NotFound($"Patient {request.PatientId} not found");

        return Result<PatientSpeciesWeightDto>.Success(
            new PatientSpeciesWeightDto(patient.Id, patient.Species, patient.WeightKg));
    }
}
