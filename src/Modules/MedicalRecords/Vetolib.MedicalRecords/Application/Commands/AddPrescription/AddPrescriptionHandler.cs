using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.AddPrescription;

internal class AddPrescriptionHandler : IRequestHandler<AddPrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public AddPrescriptionHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PrescriptionDto>> Handle(AddPrescriptionCommand cmd, CancellationToken ct)
    {
        var medicalRecord = await _context.MedicalRecords
            .FirstOrDefaultAsync(r => r.Id == cmd.MedicalRecordId, ct);

        if (medicalRecord is null)
            return Result<PrescriptionDto>.NotFound("Dossier médical introuvable");

        var prescriptionResult = Prescription.Create(
            cmd.ClinicId,
            cmd.MedicalRecordId,
            cmd.Medication,
            cmd.Dosage,
            cmd.VetLicenseNumber);

        if (!prescriptionResult.IsSuccess)
            return Result<PrescriptionDto>.Invalid(prescriptionResult.ValidationErrors.ToList());

        _context.Prescriptions.Add(prescriptionResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<PrescriptionDto>.Success(prescriptionResult.Value.ToDto());
    }
}
