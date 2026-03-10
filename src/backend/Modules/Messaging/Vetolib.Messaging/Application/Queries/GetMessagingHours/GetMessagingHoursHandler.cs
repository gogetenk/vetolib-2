using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetMessagingHours;

internal class GetMessagingHoursHandler : IRequestHandler<GetMessagingHoursQuery, Result<IReadOnlyList<MessagingHoursDto>>>
{
    private readonly MessagingDbContext _context;

    public GetMessagingHoursHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<MessagingHoursDto>>> Handle(GetMessagingHoursQuery query, CancellationToken ct)
    {
        var hours = await _context.MessagingHours
            .AsNoTracking()
            .OrderBy(h => h.DayOfWeek)
            .ToListAsync(ct);

        var dtos = hours.Select(h => h.ToDto()).ToList();

        return Result<IReadOnlyList<MessagingHoursDto>>.Success(dtos);
    }
}
