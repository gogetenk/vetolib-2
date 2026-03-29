using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetPedigree;

internal class GetPedigreeHandler : IRequestHandler<GetPedigreeQuery, Result<PedigreeNodeDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public GetPedigreeHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<PedigreeNodeDto>> Handle(GetPedigreeQuery query, CancellationToken ct)
    {
        var node = await BuildPedigreeNode(query.PatientId, query.Generations, ct);
        if (node is null)
            return Result<PedigreeNodeDto>.NotFound($"Patient '{query.PatientId}' not found");

        return Result<PedigreeNodeDto>.Success(node);
    }

    private async Task<PedigreeNodeDto?> BuildPedigreeNode(Guid patientId, int remainingGenerations, CancellationToken ct)
    {
        var patientResult = await _patientReader.GetPatientByIdAsync(patientId, ct);
        if (!patientResult.IsSuccess)
            return null;

        var patient = patientResult.Value;

        var lineage = await _context.PatientLineages
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.PatientId == patientId, ct);

        PedigreeNodeDto? motherNode = null;
        PedigreeNodeDto? fatherNode = null;

        if (remainingGenerations > 0 && lineage is not null)
        {
            if (lineage.MotherPatientId.HasValue)
                motherNode = await BuildPedigreeNode(lineage.MotherPatientId.Value, remainingGenerations - 1, ct);

            if (lineage.FatherPatientId.HasValue)
                fatherNode = await BuildPedigreeNode(lineage.FatherPatientId.Value, remainingGenerations - 1, ct);
        }

        return new PedigreeNodeDto(
            patient.Id,
            patient.Name,
            patient.Species,
            patient.Breed,
            patient.Sex,
            lineage?.RegistryNumber,
            motherNode,
            fatherNode);
    }
}
