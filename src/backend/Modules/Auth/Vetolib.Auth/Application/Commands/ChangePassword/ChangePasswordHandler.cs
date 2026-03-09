using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.ChangePassword;

internal class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly AuthDbContext _context;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(AuthDbContext context, ILogger<ChangePasswordHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand cmd, CancellationToken ct)
    {
        var user = await _context.Users
            .IgnoreQueryFilters() // EXCEPTION: auth endpoint is pre-tenant — approved in disputes.md
            .FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct);

        if (user is null)
            return Result.NotFound($"User '{cmd.UserId}' not found.");

        var changeResult = user.ChangePassword(cmd.CurrentPassword, cmd.NewPassword);
        if (!changeResult.IsSuccess)
            return changeResult;

        // Revoke all existing refresh tokens to force re-login
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == cmd.UserId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            var revokeResult = token.Revoke();
            if (!revokeResult.IsSuccess)
            {
                _logger.LogWarning("ChangePasswordHandler: failed to revoke token {TokenId} for user {UserId} — {Errors}",
                    token.Id, cmd.UserId, string.Join("; ", revokeResult.Errors));
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
