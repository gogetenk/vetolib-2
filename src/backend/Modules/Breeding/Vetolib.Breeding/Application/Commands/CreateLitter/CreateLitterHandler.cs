using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Commands.CreateLitter;

internal class CreateLitterHandler : IRequestHandler<CreateLitterCommand, Result<LitterDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public CreateLitterHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<LitterDto>> Handle(CreateLitterCommand cmd, CancellationToken ct)
    {
        // Validate mother exists and is female
        var motherResult = await _patientReader.GetPatientBasicInfoAsync(cmd.MotherPatientId, ct);
        if (!motherResult.IsSuccess)
            return Result<LitterDto>.NotFound($"Mother patient '{cmd.MotherPatientId}' not found");

        var mother = motherResult.Value;
        if (mother.Sex != Sex.Female && mother.Sex != Sex.SpayedFemale)
            return Result<LitterDto>.Error("Only female patients can be registered as mothers");

        // Validate father if specified
        if (cmd.FatherPatientId.HasValue)
        {
            var fatherResult = await _patientReader.GetPatientBasicInfoAsync(cmd.FatherPatientId.Value, ct);
            if (!fatherResult.IsSuccess)
                return Result<LitterDto>.NotFound($"Father patient '{cmd.FatherPatientId}' not found");

            var father = fatherResult.Value;
            if (father.Species != mother.Species)
                return Result<LitterDto>.Error("Father and mother must be the same species");
        }

        // Create domain entity
        var litterResult = Litter.Create(
            cmd.ClinicId,
            cmd.MotherPatientId,
            cmd.FatherPatientId,
            cmd.ExternalFatherName,
            cmd.BirthDate,
            cmd.BornCount,
            cmd.AliveCount,
            cmd.Notes);

        if (!litterResult.IsSuccess)
            return litterResult.Map(_ => (LitterDto)null!);

        await _context.Litters.AddAsync(litterResult.Value, ct);
        await _context.SaveChangesAsync(ct);

        return Result<LitterDto>.Success(litterResult.Value.ToDto());
    }
}
