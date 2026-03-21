using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;

internal class AddMedicalRecordHandler : IRequestHandler<AddMedicalRecordCommand, Result<MedicalRecordDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public AddMedicalRecordHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MedicalRecordDto>> Handle(AddMedicalRecordCommand cmd, CancellationToken ct)
    {
        // Verify patient exists in this clinic
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == cmd.PatientId, ct);

        if (patient is null)
            return Result<MedicalRecordDto>.NotFound("Patient not found");

        var recordResult = MedicalRecord.Create(
            cmd.ClinicId,
            cmd.PatientId,
            cmd.Diagnosis,
            cmd.Treatment,
            cmd.VetName,
            DateTime.UtcNow);

        if (!recordResult.IsSuccess)
            return Result<MedicalRecordDto>.Invalid(recordResult.ValidationErrors.ToList());

        _context.MedicalRecords.Add(recordResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<MedicalRecordDto>.Success(recordResult.Value.ToDto());
    }
}
