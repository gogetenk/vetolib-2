using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.AddWeightEntry;

internal class AddWeightEntryHandler : IRequestHandler<AddWeightEntryCommand, Result<WeightEntryDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public AddWeightEntryHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WeightEntryDto>> Handle(AddWeightEntryCommand cmd, CancellationToken ct)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == cmd.PatientId, ct);

        if (patient is null)
            return Result<WeightEntryDto>.NotFound($"Patient '{cmd.PatientId}' not found.");

        var entryResult = WeightEntry.Create(
            cmd.ClinicId,
            cmd.PatientId,
            cmd.WeightKg,
            cmd.RecordedBy,
            cmd.Note);

        if (!entryResult.IsSuccess)
            return Result<WeightEntryDto>.Invalid(entryResult.ValidationErrors.ToList());

        // Update the patient's current weight
        var setWeightResult = patient.SetWeight(cmd.WeightKg);
        if (!setWeightResult.IsSuccess)
            return Result<WeightEntryDto>.Invalid(setWeightResult.ValidationErrors.ToList());

        _context.WeightEntries.Add(entryResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<WeightEntryDto>.Success(entryResult.Value.ToDto());
    }
}
