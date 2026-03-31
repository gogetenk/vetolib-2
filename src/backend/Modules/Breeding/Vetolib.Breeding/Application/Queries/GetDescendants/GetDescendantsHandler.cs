using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetDescendants;

internal class GetDescendantsHandler : IRequestHandler<GetDescendantsQuery, Result<IReadOnlyList<PedigreeNodeDto>>>
{
    private const int MaxDepth = 5;
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public GetDescendantsHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<IReadOnlyList<PedigreeNodeDto>>> Handle(GetDescendantsQuery query, CancellationToken ct)
    {
        // 1 query: load ALL lineage records for the current clinic (tenant filter applied automatically)
        var allLineages = await _context.PatientLineages
            .AsNoTracking()
            .ToListAsync(ct);

        // Build a lookup: parentId -> children lineages
        var childrenByParent = new Dictionary<Guid, List<PatientLineage>>();
        foreach (var lineage in allLineages)
        {
            if (lineage.MotherPatientId.HasValue)
            {
                if (!childrenByParent.TryGetValue(lineage.MotherPatientId.Value, out var motherChildren))
                {
                    motherChildren = [];
                    childrenByParent[lineage.MotherPatientId.Value] = motherChildren;
                }
                motherChildren.Add(lineage);
            }

            if (lineage.FatherPatientId.HasValue)
            {
                if (!childrenByParent.TryGetValue(lineage.FatherPatientId.Value, out var fatherChildren))
                {
                    fatherChildren = [];
                    childrenByParent[lineage.FatherPatientId.Value] = fatherChildren;
                }
                fatherChildren.Add(lineage);
            }
        }

        // Collect all descendant patient IDs by traversing the in-memory tree
        var descendantIds = new HashSet<Guid>();
        CollectDescendantIds(query.PatientId, childrenByParent, descendantIds, MaxDepth);

        if (descendantIds.Count == 0)
            return Result<IReadOnlyList<PedigreeNodeDto>>.Success(Array.Empty<PedigreeNodeDto>());

        // 1 query: batch-load all patient details
        var patientsResult = await _patientReader.GetPatientsByIdsAsync(descendantIds.ToList(), ct);
        if (!patientsResult.IsSuccess)
            return Result<IReadOnlyList<PedigreeNodeDto>>.Success(Array.Empty<PedigreeNodeDto>());

        var patientsById = patientsResult.Value;

        // Build the lineage lookup by PatientId for registry numbers
        var lineageByPatientId = allLineages.ToDictionary(l => l.PatientId);

        // Build the flat result list
        var result = new List<PedigreeNodeDto>();
        BuildDescendantList(query.PatientId, childrenByParent, patientsById, lineageByPatientId, result, MaxDepth);

        return Result<IReadOnlyList<PedigreeNodeDto>>.Success(result);
    }

    private static void CollectDescendantIds(
        Guid parentId,
        Dictionary<Guid, List<PatientLineage>> childrenByParent,
        HashSet<Guid> collected,
        int remainingDepth)
    {
        if (remainingDepth <= 0) return;
        if (!childrenByParent.TryGetValue(parentId, out var children)) return;

        foreach (var child in children)
        {
            if (collected.Add(child.PatientId))
            {
                CollectDescendantIds(child.PatientId, childrenByParent, collected, remainingDepth - 1);
            }
        }
    }

    private static void BuildDescendantList(
        Guid parentId,
        Dictionary<Guid, List<PatientLineage>> childrenByParent,
        IReadOnlyDictionary<Guid, PatientDto> patientsById,
        Dictionary<Guid, PatientLineage> lineageByPatientId,
        List<PedigreeNodeDto> result,
        int remainingDepth)
    {
        if (remainingDepth <= 0) return;
        if (!childrenByParent.TryGetValue(parentId, out var children)) return;

        foreach (var child in children)
        {
            if (!patientsById.TryGetValue(child.PatientId, out var patient)) continue;

            lineageByPatientId.TryGetValue(child.PatientId, out var lineage);

            // For descendants, we don't fill Mother/Father nodes (we go DOWN, not UP)
            var node = new PedigreeNodeDto(
                patient.Id,
                patient.Name,
                patient.Species,
                patient.Breed,
                patient.Sex,
                lineage?.RegistryNumber,
                null,
                null);

            result.Add(node);
            BuildDescendantList(child.PatientId, childrenByParent, patientsById, lineageByPatientId, result, remainingDepth - 1);
        }
    }
}
