using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetDescendants;

internal class GetDescendantsHandler : IRequestHandler<GetDescendantsQuery, Result<IReadOnlyList<PedigreeNodeDto>>>
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public GetDescendantsHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<IReadOnlyList<PedigreeNodeDto>>> Handle(GetDescendantsQuery query, CancellationToken ct)
    {
        var descendants = await CollectDescendants(query.PatientId, 5, ct);
        return Result<IReadOnlyList<PedigreeNodeDto>>.Success(descendants);
    }

    private async Task<List<PedigreeNodeDto>> CollectDescendants(Guid parentId, int maxDepth, CancellationToken ct)
    {
        if (maxDepth <= 0) return [];

        var childLineages = await _context.PatientLineages
            .AsNoTracking()
            .Where(l => l.MotherPatientId == parentId || l.FatherPatientId == parentId)
            .ToListAsync(ct);

        var result = new List<PedigreeNodeDto>();

        foreach (var child in childLineages)
        {
            var patientResult = await _patientReader.GetPatientByIdAsync(child.PatientId, ct);
            if (!patientResult.IsSuccess) continue;

            var patient = patientResult.Value;

            var grandchildren = await CollectDescendants(child.PatientId, maxDepth - 1, ct);

            // For descendants, we don't fill Mother/Father nodes (we go DOWN, not UP)
            var node = new PedigreeNodeDto(
                patient.Id,
                patient.Name,
                patient.Species,
                patient.Breed,
                patient.Sex,
                child.RegistryNumber,
                null,
                null);

            result.Add(node);
            result.AddRange(grandchildren);
        }

        return result;
    }
}
