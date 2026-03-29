using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Breeding.Application.Commands.RecordHeatCycle;

internal class RecordHeatCycleHandler : IRequestHandler<RecordHeatCycleCommand, Result<HeatCycleDto>>
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;

    public RecordHeatCycleHandler(BreedingDbContext context, IPatientReader patientReader)
    {
        _context = context;
        _patientReader = patientReader;
    }

    public async Task<Result<HeatCycleDto>> Handle(RecordHeatCycleCommand cmd, CancellationToken ct)
    {
        // Validate patient sex via cross-module reader
        var sexResult = await _patientReader.GetPatientSexAsync(cmd.PatientId, ct);
        if (!sexResult.IsSuccess)
            return Result<HeatCycleDto>.NotFound($"Patient '{cmd.PatientId}' not found");

        var sex = sexResult.Value;

        if (sex == Sex.Male || sex == Sex.NeuteredMale)
            return Result<HeatCycleDto>.Error("Only female patients can have heat cycles recorded");

        if (sex == Sex.SpayedFemale)
            return Result<HeatCycleDto>.Error("Spayed patients do not have heat cycles");

        // Check for overlapping cycles
        if (cmd.EndDate.HasValue)
        {
            var overlapping = await _context.HeatCycles
                .AnyAsync(h => h.PatientId == cmd.PatientId
                    && h.StartDate <= cmd.EndDate.Value
                    && (h.EndDate == null || h.EndDate.Value >= cmd.StartDate), ct);

            if (overlapping)
                return Result<HeatCycleDto>.Conflict("This heat cycle overlaps with an existing one");
        }

        // Create via domain factory
        var cycleResult = HeatCycle.Create(cmd.ClinicId, cmd.PatientId, cmd.StartDate, cmd.EndDate, cmd.Notes);
        if (!cycleResult.IsSuccess)
            return Result<HeatCycleDto>.Invalid(cycleResult.ValidationErrors.ToList());

        _context.HeatCycles.Add(cycleResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<HeatCycleDto>.Success(cycleResult.Value.ToDto());
    }
}
