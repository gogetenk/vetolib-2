using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.UpdatePatient;

internal class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public UpdatePatientHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand cmd, CancellationToken ct)
    {
        var patient = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == cmd.PatientId, ct);

        if (patient is null)
            return Result<PatientDto>.NotFound($"Patient '{cmd.PatientId}' not found.");

        // Check microchip uniqueness within clinic
        if (cmd.MicrochipNumber is not null)
        {
            var exists = await _context.Patients
                .AnyAsync(p => p.MicrochipNumber == cmd.MicrochipNumber && p.Id != cmd.PatientId, ct);
            if (exists)
                return Result<PatientDto>.Conflict("A patient with this microchip number already exists");
        }

        // Update patient fields
        var updateResult = patient.UpdateInfo(cmd.Name, cmd.Species, cmd.Breed, cmd.BirthDate, cmd.MicrochipNumber);
        if (!updateResult.IsSuccess)
            return Result<PatientDto>.Error(string.Join("; ", updateResult.Errors));

        // Update owner info if provided
        if (cmd.OwnerPhone is not null || cmd.OwnerName is not null)
        {
            var ownerLink = patient.PatientOwners.FirstOrDefault();
            if (ownerLink?.Owner is not null)
            {
                var owner = ownerLink.Owner;
                if (cmd.OwnerPhone is not null)
                {
                    var phoneResult = owner.UpdatePhone(cmd.OwnerPhone);
                    if (!phoneResult.IsSuccess)
                        return Result<PatientDto>.Error(string.Join("; ", phoneResult.Errors));
                }
                if (cmd.OwnerName is not null)
                {
                    var nameResult = owner.UpdateName(cmd.OwnerName);
                    if (!nameResult.IsSuccess)
                        return Result<PatientDto>.Error(string.Join("; ", nameResult.Errors));
                }
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result<PatientDto>.Success(patient.ToDto());
    }
}
