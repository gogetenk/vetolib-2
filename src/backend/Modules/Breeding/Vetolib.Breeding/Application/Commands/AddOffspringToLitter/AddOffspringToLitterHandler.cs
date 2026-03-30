using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Commands.AddOffspringToLitter;

internal class AddOffspringToLitterHandler : IRequestHandler<AddOffspringToLitterCommand, Result<LitterDto>>
{
    private readonly BreedingDbContext _context;

    public AddOffspringToLitterHandler(BreedingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LitterDto>> Handle(AddOffspringToLitterCommand cmd, CancellationToken ct)
    {
        var litter = await _context.Litters
            .Include(l => l.Offspring)
            .FirstOrDefaultAsync(l => l.Id == cmd.LitterId, ct);

        if (litter is null)
            return Result<LitterDto>.NotFound($"Litter '{cmd.LitterId}' not found");

        var addResult = litter.AddOffspring(cmd.PatientId, cmd.BirthOrder);
        if (!addResult.IsSuccess)
            return Result<LitterDto>.Error(string.Join("; ", addResult.Errors));

        // Auto-link lineage: create or update PatientLineage for the offspring
        await AutoLinkLineage(litter.ClinicId, cmd.PatientId, litter.MotherPatientId, litter.FatherPatientId, ct);

        await _context.SaveChangesAsync(ct);

        return Result<LitterDto>.Success(litter.ToDto());
    }

    private async Task AutoLinkLineage(
        Guid clinicId, Guid offspringId, Guid motherId, Guid? fatherId, CancellationToken ct)
    {
        var existing = await _context.PatientLineages
            .FirstOrDefaultAsync(l => l.PatientId == offspringId, ct);

        if (existing is not null)
        {
            existing.SetParents(motherId, fatherId);
        }
        else
        {
            var lineageResult = PatientLineage.Create(
                clinicId, offspringId, motherId, fatherId, null, null);

            if (lineageResult.IsSuccess)
                await _context.PatientLineages.AddAsync(lineageResult.Value, ct);
        }
    }
}
