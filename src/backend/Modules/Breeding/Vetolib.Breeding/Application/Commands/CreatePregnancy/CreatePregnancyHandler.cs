using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Domain;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Application.Commands.CreatePregnancy;

internal class CreatePregnancyHandler : IRequestHandler<CreatePregnancyCommand, Result<PregnancyDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IClinicContext _clinicContext;
    private readonly IPatientReader _patientReader;

    public CreatePregnancyHandler(BreedingDbContext context, IClinicContext clinicContext, IPatientReader patientReader)
    {
        _context = context;
        _clinicContext = clinicContext;
        _patientReader = patientReader;
    }

    public async Task<Result<PregnancyDto>> Handle(CreatePregnancyCommand cmd, CancellationToken ct)
    {
        // Resolve patient sex and species via cross-module reader
        var patientResult = await _patientReader.GetPatientBasicInfoAsync(cmd.PatientId, ct);
        if (!patientResult.IsSuccess)
            return Result<PregnancyDto>.NotFound($"Patient '{cmd.PatientId}' not found");

        var patientInfo = patientResult.Value;

        // Validate sex — only female patients can have pregnancies
        if (patientInfo.Sex == Sex.Male || patientInfo.Sex == Sex.NeuteredMale)
            return Result<PregnancyDto>.Error("Only female patients can have pregnancies");

        // Validate spayed patients cannot be pregnant
        if (patientInfo.Sex == Sex.SpayedFemale)
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
            patientInfo.Species.ToString(),
            cmd.Notes);

        if (!pregnancyResult.IsSuccess)
            return pregnancyResult.Map(_ => (PregnancyDto)null!);

        _context.Pregnancies.Add(pregnancyResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<PregnancyDto>.Success(pregnancyResult.Value.ToDto());
    }
}
