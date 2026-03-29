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
        // Check microchip uniqueness within clinic
        if (cmd.MicrochipNumber is not null)
        {
            var exists = await _context.Patients
                .AnyAsync(p => p.MicrochipNumber == cmd.MicrochipNumber, ct);
            if (exists)
                return Result<PatientDto>.Conflict("A patient with this microchip number already exists");
        }

        // Create patient via domain factory
        var patientResult = Patient.Create(cmd.ClinicId, cmd.Name, cmd.Species, cmd.Breed, cmd.BirthDate, cmd.MicrochipNumber);

        if (!patientResult.IsSuccess)
            return Result<PatientDto>.Invalid(patientResult.ValidationErrors.ToList());

        var patient = patientResult.Value;

        // Parse owner name: "Faisal Al-Kuwari" → FirstName="Faisal", LastName="Al-Kuwari"
        var nameParts = cmd.OwnerName.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : "-";

        // Generate a unique email for the owner using phone (since email is required but not provided here)
        var ownerEmail = $"owner.{cmd.OwnerPhone.Replace(" ", "").Replace("+", "").Replace("-", "")}@vetoclinic.ae";

        // Try to find existing owner by phone in this clinic
        var existingOwner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Phone == cmd.OwnerPhone, ct);

        Owner owner;
        if (existingOwner is not null)
        {
            owner = existingOwner;
        }
        else
        {
            var ownerResult = Owner.Create(cmd.ClinicId, firstName, lastName, ownerEmail, cmd.OwnerPhone);
            if (!ownerResult.IsSuccess)
                return Result<PatientDto>.Invalid(ownerResult.ValidationErrors.ToList());

            owner = ownerResult.Value;
            _context.Owners.Add(owner);
        }

        // Link owner to patient
        var patientOwner = PatientOwner.Create(cmd.ClinicId, patient.Id, owner.Id);
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
