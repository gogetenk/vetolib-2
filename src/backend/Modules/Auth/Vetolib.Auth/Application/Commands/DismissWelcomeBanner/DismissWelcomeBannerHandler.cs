using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.DismissWelcomeBanner;

internal class DismissWelcomeBannerHandler : IRequestHandler<DismissWelcomeBannerCommand, Result>
{
    private readonly AuthDbContext _context;

    public DismissWelcomeBannerHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DismissWelcomeBannerCommand command, CancellationToken ct)
    {
        var state = await _context.OnboardingStates
            .FirstOrDefaultAsync(o => o.UserId == command.UserId, ct);

        if (state is null)
            return Result.NotFound("Onboarding state not found.");

        var result = state.DismissBanner();
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
