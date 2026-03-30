using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.VerifyEmail;

internal class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly AuthDbContext _context;

    public VerifyEmailHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(VerifyEmailCommand cmd, CancellationToken ct)
    {
        // Verification is cross-tenant — token lookup must ignore tenant filters
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == cmd.Token, ct);

        if (user is null)
            return Result.Error("INVALID_TOKEN:Invalid or expired verification token");

        var result = user.VerifyEmail(cmd.Token);
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
