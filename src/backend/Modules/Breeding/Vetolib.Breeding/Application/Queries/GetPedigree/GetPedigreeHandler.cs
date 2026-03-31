using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
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
        // Step 1: Batch-load ALL lineages for this clinic in one query.
        // The multi-tenant filter already scopes to the current clinic.
        var allLineages = await _context.PatientLineages
            .AsNoTracking()
            .ToListAsync(ct);

        var lineageByPatient = allLineages.ToDictionary(l => l.PatientId);

        // Step 2: Walk the tree in-memory to collect all patient IDs we need.
        var patientIds = new HashSet<Guid>();
        CollectPatientIds(query.PatientId, query.Generations, lineageByPatient, patientIds);

        if (patientIds.Count == 0)
            return Result<PedigreeNodeDto>.NotFound($"Patient '{query.PatientId}' not found");

        // Step 3: Batch-load all needed patients in one query.
        var patientsResult = await _patientReader.GetPatientsByIdsAsync(patientIds.ToList(), ct);
        if (!patientsResult.IsSuccess)
            return Result<PedigreeNodeDto>.Error("Failed to load patients for pedigree");

        var patients = patientsResult.Value;

        // Step 4: Build the tree from in-memory data.
        var node = BuildPedigreeNode(query.PatientId, query.Generations, lineageByPatient, patients);
        if (node is null)
            return Result<PedigreeNodeDto>.NotFound($"Patient '{query.PatientId}' not found");

        return Result<PedigreeNodeDto>.Success(node);
    }

    private static void CollectPatientIds(
        Guid patientId,
        int remainingGenerations,
        Dictionary<Guid, PatientLineage> lineageByPatient,
        HashSet<Guid> collected)
    {
        if (!collected.Add(patientId))
            return; // already visited — avoid infinite loops from bad data

        if (remainingGenerations <= 0)
            return;

        if (!lineageByPatient.TryGetValue(patientId, out var lineage))
            return;

        if (lineage.MotherPatientId.HasValue)
            CollectPatientIds(lineage.MotherPatientId.Value, remainingGenerations - 1, lineageByPatient, collected);

        if (lineage.FatherPatientId.HasValue)
            CollectPatientIds(lineage.FatherPatientId.Value, remainingGenerations - 1, lineageByPatient, collected);
    }

    private static PedigreeNodeDto? BuildPedigreeNode(
        Guid patientId,
        int remainingGenerations,
        Dictionary<Guid, PatientLineage> lineageByPatient,
        IReadOnlyDictionary<Guid, PatientDto> patients)
    {
        if (!patients.TryGetValue(patientId, out var patient))
            return null;

        lineageByPatient.TryGetValue(patientId, out var lineage);

        PedigreeNodeDto? motherNode = null;
        PedigreeNodeDto? fatherNode = null;

        if (remainingGenerations > 0 && lineage is not null)
        {
            if (lineage.MotherPatientId.HasValue)
                motherNode = BuildPedigreeNode(lineage.MotherPatientId.Value, remainingGenerations - 1, lineageByPatient, patients);

            if (lineage.FatherPatientId.HasValue)
                fatherNode = BuildPedigreeNode(lineage.FatherPatientId.Value, remainingGenerations - 1, lineageByPatient, patients);
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
