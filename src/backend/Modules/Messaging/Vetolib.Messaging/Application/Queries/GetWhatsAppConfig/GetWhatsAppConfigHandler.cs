using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Queries.GetWhatsAppConfig;

internal sealed class GetWhatsAppConfigHandler : IRequestHandler<GetWhatsAppConfigQuery, Result<WhatsAppConfigDto>>
{
    private readonly MessagingDbContext _context;
    private readonly IClinicContext _clinicContext;

    public GetWhatsAppConfigHandler(MessagingDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<WhatsAppConfigDto>> Handle(GetWhatsAppConfigQuery request, CancellationToken ct)
    {
        var waba = await _context.WhatsAppBusinessAccounts
            .FirstOrDefaultAsync(w => w.ClinicId == _clinicContext.ClinicId, ct);

        if (waba is null)
            return Result<WhatsAppConfigDto>.NotFound("WhatsApp configuration not found");

        return Result<WhatsAppConfigDto>.Success(waba.ToDto());
    }
}
