using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Commands.LinkOwnerByMicrochip;

internal class LinkOwnerByMicrochipHandler : IRequestHandler<LinkOwnerByMicrochipCommand, Result>
{
    private readonly AuthDbContext _context;
    private readonly IOwnerAccountLinker _ownerAccountLinker;

    public LinkOwnerByMicrochipHandler(AuthDbContext context, IOwnerAccountLinker ownerAccountLinker)
    {
        _context = context;
        _ownerAccountLinker = ownerAccountLinker;
    }

    public async Task<Result> Handle(LinkOwnerByMicrochipCommand cmd, CancellationToken ct)
    {
        // Verify OwnerAccount exists
        var accountExists = await _context.OwnerAccounts
            .AnyAsync(a => a.Id == cmd.OwnerAccountId, ct);

        if (!accountExists)
            return Result.NotFound("ACCOUNT_NOT_FOUND:Owner account not found");

        var clinicId = await _ownerAccountLinker.LinkOwnerByMicrochipAsync(
            cmd.OwnerAccountId, cmd.MicrochipNumber, ct);

        if (clinicId is null)
            return Result.NotFound("MICROCHIP_NOT_FOUND:No patient found with this microchip number");

        return Result.Success();
    }
}
