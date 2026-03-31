using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Commands.RegisterWebhook;

internal class RegisterWebhookHandler : IRequestHandler<RegisterWebhookCommand, Result<WebhookRegistrationDto>>
{
    private readonly AuthDbContext _context;
    private readonly IClinicContext _clinicContext;

    public RegisterWebhookHandler(AuthDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<WebhookRegistrationDto>> Handle(RegisterWebhookCommand cmd, CancellationToken ct)
    {
        var result = WebhookRegistration.Create(
            _clinicContext.ClinicId,
            cmd.Name,
            cmd.Secret,
            cmd.EventTypes);

        if (!result.IsSuccess)
            return Result<WebhookRegistrationDto>.Invalid(result.ValidationErrors.ToList());

        _context.WebhookRegistrations.Add(result.Value);
        await _context.SaveChangesAsync(ct);

        return Result<WebhookRegistrationDto>.Success(result.Value.ToDto());
    }
}
