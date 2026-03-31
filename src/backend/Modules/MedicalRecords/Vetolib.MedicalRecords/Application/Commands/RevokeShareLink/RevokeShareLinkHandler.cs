using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.RevokeShareLink;

internal class RevokeShareLinkHandler : IRequestHandler<RevokeShareLinkCommand, Result>
{
    private readonly MedicalRecordsDbContext _context;

    public RevokeShareLinkHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RevokeShareLinkCommand cmd, CancellationToken ct)
    {
        var link = await _context.SharedRecordLinks
            .FirstOrDefaultAsync(l => l.Id == cmd.LinkId, ct);

        if (link is null)
            return Result.NotFound($"Share link '{cmd.LinkId}' not found.");

        if (link.OwnerAccountId != cmd.OwnerAccountId)
            return Result.Forbidden();

        var revokeResult = link.Revoke();
        if (!revokeResult.IsSuccess)
            return revokeResult;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
