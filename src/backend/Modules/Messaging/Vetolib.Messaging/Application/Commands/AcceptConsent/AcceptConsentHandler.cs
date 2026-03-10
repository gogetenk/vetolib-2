using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.AcceptConsent;

internal class AcceptConsentHandler : IRequestHandler<AcceptConsentCommand, Result>
{
    private readonly MessagingDbContext _context;

    public AcceptConsentHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AcceptConsentCommand request, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters: portal auth bypasses JWT tenant context
        var token = await _context.OwnerPortalTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                t => t.OwnerId == request.OwnerId && t.ClinicId == request.ClinicId,
                cancellationToken);

        if (token is null)
            return Result.NotFound("Portal token not found");

        var result = token.RecordConsent(request.ConsentVersion);
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
