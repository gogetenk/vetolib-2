using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetAppointmentCount;

internal class GetAppointmentCountHandler : IRequestHandler<GetAppointmentCountQuery, Result<int>>
{
    private readonly AgendaDbContext _context;

    public GetAppointmentCountHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(GetAppointmentCountQuery query, CancellationToken ct)
    {
        var count = await _context.Appointments
            .AsNoTracking()
            .CountAsync(ct);

        return Result<int>.Success(count);
    }
}
