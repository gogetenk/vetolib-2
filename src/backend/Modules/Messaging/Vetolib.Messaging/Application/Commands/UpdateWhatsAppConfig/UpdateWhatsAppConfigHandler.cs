using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.UpdateWhatsAppConfig;

internal sealed class UpdateWhatsAppConfigHandler
    : IRequestHandler<UpdateWhatsAppConfigCommand, Result<WhatsAppConfigDto>>
{
    private readonly MessagingDbContext _context;
    private readonly IClinicContext _clinicContext;
    private readonly ITokenEncryptor _tokenEncryptor;

    public UpdateWhatsAppConfigHandler(
        MessagingDbContext context,
        IClinicContext clinicContext,
        ITokenEncryptor tokenEncryptor)
    {
        _context = context;
        _clinicContext = clinicContext;
        _tokenEncryptor = tokenEncryptor;
    }

    public async Task<Result<WhatsAppConfigDto>> Handle(UpdateWhatsAppConfigCommand request, CancellationToken ct)
    {
        var encryptedToken = _tokenEncryptor.Encrypt(request.AccessToken);

        var existing = await _context.WhatsAppBusinessAccounts
            .FirstOrDefaultAsync(w => w.ClinicId == _clinicContext.ClinicId, ct);

        if (existing is not null)
        {
            var updateResult = existing.Update(request.WabaId, request.PhoneNumberId, encryptedToken);
            if (!updateResult.IsSuccess)
                return Result<WhatsAppConfigDto>.Invalid(updateResult.ValidationErrors.ToList());

            await _context.SaveChangesAsync(ct);
            return Result<WhatsAppConfigDto>.Success(existing.ToDto());
        }

        var createResult = WhatsAppBusinessAccount.Create(
            _clinicContext.ClinicId,
            request.WabaId,
            request.PhoneNumberId,
            encryptedToken);

        if (!createResult.IsSuccess)
            return Result<WhatsAppConfigDto>.Invalid(createResult.ValidationErrors.ToList());

        _context.WhatsAppBusinessAccounts.Add(createResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<WhatsAppConfigDto>.Success(createResult.Value.ToDto());
    }
}
