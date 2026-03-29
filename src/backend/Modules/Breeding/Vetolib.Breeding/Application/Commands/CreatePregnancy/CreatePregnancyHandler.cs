using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Domain;
using Vetolib.Breeding.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Application.Commands.CreatePregnancy;

internal class CreatePregnancyHandler : IRequestHandler<CreatePregnancyCommand, Result<PregnancyDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IClinicContext _clinicContext;

    public CreatePregnancyHandler(BreedingDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<PregnancyDto>> Handle(CreatePregnancyCommand cmd, CancellationToken ct)
    {
        // Validate sex — only female patients can have pregnancies
        var sex = cmd.PatientSex.ToUpperInvariant();
        if (sex is "MALE" or "NEUTEREDMALE")
            return Result<PregnancyDto>.Error("Only female patients can have pregnancies");

        // Validate spayed patients cannot be pregnant
        if (sex is "SPAYEDFEMALE")
            return Result<PregnancyDto>.Error("Spayed patients cannot be pregnant");

        // Check for overlapping active pregnancies
        var hasActivePregnancy = await _context.Pregnancies
            .AnyAsync(p => p.PatientId == cmd.PatientId && p.Status == PregnancyStatus.Active, ct);

        if (hasActivePregnancy)
            return Result<PregnancyDto>.Error("Patient already has an active pregnancy");

        var pregnancyResult = Pregnancy.Create(
            _clinicContext.ClinicId,
            cmd.PatientId,
            cmd.FatherPatientId,
            cmd.MatingDate,
            cmd.MatingMethod,
            cmd.PatientSpecies,
            cmd.Notes);

        if (!pregnancyResult.IsSuccess)
            return pregnancyResult.Map(_ => (PregnancyDto)null!);

        _context.Pregnancies.Add(pregnancyResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<PregnancyDto>.Success(pregnancyResult.Value.ToDto());
    }
}
