using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Commands.SetLineage;

internal class SetLineageHandler : IRequestHandler<SetLineageCommand, Result<PatientLineageDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public SetLineageHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<PatientLineageDto>> Handle(SetLineageCommand cmd, CancellationToken ct)
    {
        // Validate offspring exists
        var offspringResult = await _patientReader.GetPatientBasicInfoAsync(cmd.PatientId, ct);
        if (!offspringResult.IsSuccess)
            return Result<PatientLineageDto>.NotFound($"Patient '{cmd.PatientId}' not found");

        var offspring = offspringResult.Value;

        // Validate mother if specified
        PatientBasicInfoDto? motherInfo = null;
        if (cmd.MotherPatientId.HasValue)
        {
            var motherResult = await _patientReader.GetPatientBasicInfoAsync(cmd.MotherPatientId.Value, ct);
            if (!motherResult.IsSuccess)
                return Result<PatientLineageDto>.NotFound($"Mother patient '{cmd.MotherPatientId}' not found");
            motherInfo = motherResult.Value;
        }

        // Validate father if specified
        PatientBasicInfoDto? fatherInfo = null;
        if (cmd.FatherPatientId.HasValue)
        {
            var fatherResult = await _patientReader.GetPatientBasicInfoAsync(cmd.FatherPatientId.Value, ct);
            if (!fatherResult.IsSuccess)
                return Result<PatientLineageDto>.NotFound($"Father patient '{cmd.FatherPatientId}' not found");
            fatherInfo = fatherResult.Value;
        }

        // Validate sex and species compatibility
        var compatResult = PatientLineage.ValidateParentCompatibility(offspring, motherInfo, fatherInfo);
        if (!compatResult.IsSuccess)
            return Result<PatientLineageDto>.Error(string.Join("; ", compatResult.Errors));

        // Circular lineage check
        var circularCheck = await CheckCircularLineage(cmd.PatientId, cmd.MotherPatientId, cmd.FatherPatientId, ct);
        if (!circularCheck.IsSuccess)
            return Result<PatientLineageDto>.Error(string.Join("; ", circularCheck.Errors));

        // Upsert
        var existing = await _context.PatientLineages
            .FirstOrDefaultAsync(l => l.PatientId == cmd.PatientId, ct);

        if (existing is not null)
        {
            var setResult = existing.SetParents(cmd.MotherPatientId, cmd.FatherPatientId);
            if (!setResult.IsSuccess)
                return Result<PatientLineageDto>.Error(string.Join("; ", setResult.Errors));

            existing.SetRegistry(cmd.RegistryNumber, cmd.RegistryType);
        }
        else
        {
            var createResult = PatientLineage.Create(
                cmd.ClinicId,
                cmd.PatientId,
                cmd.MotherPatientId,
                cmd.FatherPatientId,
                cmd.RegistryNumber,
                cmd.RegistryType);

            if (!createResult.IsSuccess)
                return createResult.Map(_ => (PatientLineageDto)null!);

            await _context.PatientLineages.AddAsync(createResult.Value, ct);
        }

        await _context.SaveChangesAsync(ct);

        return Result<PatientLineageDto>.Success(new PatientLineageDto(
            cmd.PatientId,
            offspring.Name,
            cmd.MotherPatientId,
            motherInfo?.Name,
            cmd.FatherPatientId,
            fatherInfo?.Name,
            cmd.RegistryNumber,
            cmd.RegistryType));
    }

    private async Task<Result> CheckCircularLineage(
        Guid patientId, Guid? motherId, Guid? fatherId, CancellationToken ct)
    {
        // Check if any proposed parent has this patient as an ancestor
        var parentIds = new List<Guid>();
        if (motherId.HasValue) parentIds.Add(motherId.Value);
        if (fatherId.HasValue) parentIds.Add(fatherId.Value);

        foreach (var parentId in parentIds)
        {
            if (await IsDescendantOf(parentId, patientId, 10, ct))
                return Result.Error("Circular lineage detected: the proposed parent is a descendant of this patient");
        }

        return Result.Success();
    }

    /// <summary>
    /// Checks if <paramref name="candidateDescendant"/> has <paramref name="ancestorId"/>
    /// anywhere in its ancestor chain (up to maxDepth).
    /// </summary>
    private async Task<bool> IsDescendantOf(
        Guid candidateDescendant, Guid ancestorId, int maxDepth, CancellationToken ct)
    {
        if (maxDepth <= 0) return false;

        var lineage = await _context.PatientLineages
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.PatientId == candidateDescendant, ct);

        if (lineage is null) return false;

        if (lineage.MotherPatientId == ancestorId || lineage.FatherPatientId == ancestorId)
            return true;

        if (lineage.MotherPatientId.HasValue && await IsDescendantOf(lineage.MotherPatientId.Value, ancestorId, maxDepth - 1, ct))
            return true;

        if (lineage.FatherPatientId.HasValue && await IsDescendantOf(lineage.FatherPatientId.Value, ancestorId, maxDepth - 1, ct))
            return true;

        return false;
    }
}
