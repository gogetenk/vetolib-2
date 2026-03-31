using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.DeactivateWebhook;

internal class DeactivateWebhookHandler : IRequestHandler<DeactivateWebhookCommand, Result>
{
    private readonly AuthDbContext _context;

    public DeactivateWebhookHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateWebhookCommand cmd, CancellationToken ct)
    {
        var registration = await _context.WebhookRegistrations
            .FirstOrDefaultAsync(w => w.Id == cmd.Id, ct);

        if (registration is null)
            return Result.NotFound("Webhook registration not found");

        var result = registration.Deactivate();
        if (!result.IsSuccess)
            return result;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
