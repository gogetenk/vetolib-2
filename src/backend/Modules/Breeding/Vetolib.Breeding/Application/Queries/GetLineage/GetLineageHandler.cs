using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetLineage;

internal class GetLineageHandler : IRequestHandler<GetLineageQuery, Result<PatientLineageDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public GetLineageHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<PatientLineageDto>> Handle(GetLineageQuery query, CancellationToken ct)
    {
        var lineage = await _context.PatientLineages
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.PatientId == query.PatientId, ct);

        if (lineage is null)
            return Result<PatientLineageDto>.NotFound($"No lineage found for patient '{query.PatientId}'");

        var patientResult = await _patientReader.GetPatientBasicInfoAsync(query.PatientId, ct);
        var patientName = patientResult.IsSuccess ? patientResult.Value.Name : "Unknown";

        string? motherName = null;
        if (lineage.MotherPatientId.HasValue)
        {
            var motherResult = await _patientReader.GetPatientBasicInfoAsync(lineage.MotherPatientId.Value, ct);
            motherName = motherResult.IsSuccess ? motherResult.Value.Name : null;
        }

        string? fatherName = null;
        if (lineage.FatherPatientId.HasValue)
        {
            var fatherResult = await _patientReader.GetPatientBasicInfoAsync(lineage.FatherPatientId.Value, ct);
            fatherName = fatherResult.IsSuccess ? fatherResult.Value.Name : null;
        }

        return Result<PatientLineageDto>.Success(new PatientLineageDto(
            lineage.PatientId,
            patientName,
            lineage.MotherPatientId,
            motherName,
            lineage.FatherPatientId,
            fatherName,
            lineage.RegistryNumber,
            lineage.RegistryType));
    }
}
