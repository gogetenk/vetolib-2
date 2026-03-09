using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.Logout;

internal class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly AuthDbContext _context;

    public LogoutHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == cmd.UserId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
