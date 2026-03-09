using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.CreatePatient;

internal class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public CreatePatientHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand cmd, CancellationToken ct)
    {
        // Verify owner exists in this clinic
        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == cmd.OwnerId, ct);

        if (owner is null)
            return Result<PatientDto>.Error("OWNER_NOT_FOUND:Le proprietaire n'existe pas");

        // Create patient via domain factory
        var patientResult = Patient.Create(cmd.ClinicId, cmd.Name, cmd.Species, cmd.Breed, cmd.DateOfBirth);

        if (!patientResult.IsSuccess)
            return Result<PatientDto>.Invalid(patientResult.ValidationErrors.ToList());

        var patient = patientResult.Value;

        // Create the patient-owner link
        var patientOwner = PatientOwner.Create(cmd.ClinicId, patient.Id, cmd.OwnerId);
        patient.AddOwner(patientOwner);

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(ct);

        // Reload with owner data for the DTO
        var saved = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstAsync(p => p.Id == patient.Id, ct);

        return Result<PatientDto>.Success(saved.ToDto());
    }
}
