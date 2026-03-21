using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.CreateOwner;

internal class CreateOwnerHandler : IRequestHandler<CreateOwnerCommand, Result<OwnerDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public CreateOwnerHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OwnerDto>> Handle(CreateOwnerCommand cmd, CancellationToken ct)
    {
        // Check if owner email already exists in this clinic
        var existingOwner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Email == cmd.Email.ToLowerInvariant(), ct);

        if (existingOwner is not null)
            return Result<OwnerDto>.Error("EMAIL_EXISTS:This owner email is already in use");

        var ownerResult = Owner.Create(cmd.ClinicId, cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone);

        if (!ownerResult.IsSuccess)
            return Result<OwnerDto>.Invalid(ownerResult.ValidationErrors.ToList());

        _context.Owners.Add(ownerResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<OwnerDto>.Success(ownerResult.Value.ToDto());
    }
}
