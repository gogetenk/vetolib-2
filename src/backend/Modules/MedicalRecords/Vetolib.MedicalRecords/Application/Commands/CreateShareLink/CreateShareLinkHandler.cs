using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.CreateShareLink;

internal class CreateShareLinkHandler : IRequestHandler<CreateShareLinkCommand, Result<CreateShareLinkResponse>>
{
    private const string ShareBaseUrl = "https://vetara.ae/shared/";
    private readonly MedicalRecordsDbContext _context;

    public CreateShareLinkHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateShareLinkResponse>> Handle(CreateShareLinkCommand cmd, CancellationToken ct)
    {
        // Verify the patient exists
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == cmd.PatientId, ct);

        if (!patientExists)
            return Result<CreateShareLinkResponse>.NotFound($"Patient '{cmd.PatientId}' not found.");

        // Verify the owner is linked to this patient
        var ownerLinked = await _context.Owners
            .AnyAsync(o => o.OwnerAccountId == cmd.OwnerAccountId
                && o.PatientOwners.Any(po => po.PatientId == cmd.PatientId), ct);

        if (!ownerLinked)
            return Result<CreateShareLinkResponse>.Forbidden();

        var linkResult = SharedRecordLink.Create(cmd.ClinicId, cmd.PatientId, cmd.OwnerAccountId);

        if (!linkResult.IsSuccess)
            return Result<CreateShareLinkResponse>.Invalid(linkResult.ValidationErrors.ToList());

        var link = linkResult.Value;
        _context.SharedRecordLinks.Add(link);
        await _context.SaveChangesAsync(ct);

        return Result<CreateShareLinkResponse>.Success(
            new CreateShareLinkResponse(
                link.Id,
                link.Token,
                $"{ShareBaseUrl}{link.Token}",
                link.ExpiresAt));
    }
}
