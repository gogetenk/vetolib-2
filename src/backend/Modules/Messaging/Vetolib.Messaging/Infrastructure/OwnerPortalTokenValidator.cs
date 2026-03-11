using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Infrastructure;

/// <summary>
/// Validates a magic link token string against the portal tokens stored in the Messaging database.
/// Implements the public IOwnerPortalTokenValidator contract so other modules (e.g. Agenda) can
/// validate portal tokens without directly referencing the Messaging runtime assembly.
/// </summary>
internal class OwnerPortalTokenValidator : IOwnerPortalTokenValidator
{
    private readonly MessagingDbContext _context;

    public OwnerPortalTokenValidator(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OwnerPortalTokenDto>> ValidateAsync(string tokenValue, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenValue))
            return Result<OwnerPortalTokenDto>.Unauthorized();

        // Token lookup must bypass the multi-tenant filter — tokens are global identifiers.
        var token = await _context.OwnerPortalTokens
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == tokenValue, ct);

        if (token is null)
            return Result<OwnerPortalTokenDto>.Unauthorized();

        if (!token.IsValid())
            return Result<OwnerPortalTokenDto>.Unauthorized();

        return Result<OwnerPortalTokenDto>.Success(token.ToDto());
    }
}
