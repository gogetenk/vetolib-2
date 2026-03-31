using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.ListWebhooks;

internal class ListWebhooksHandler : IRequestHandler<ListWebhooksQuery, Result<List<WebhookRegistrationDto>>>
{
    private readonly AuthDbContext _context;

    public ListWebhooksHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WebhookRegistrationDto>>> Handle(ListWebhooksQuery query, CancellationToken ct)
    {
        var registrations = await _context.WebhookRegistrations
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);

        var dtos = registrations.Select(w => w.ToDto()).ToList();
        return Result<List<WebhookRegistrationDto>>.Success(dtos);
    }
}
