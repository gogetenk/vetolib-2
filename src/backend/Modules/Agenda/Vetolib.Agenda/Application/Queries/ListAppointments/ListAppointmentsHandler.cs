using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListAppointments;

internal class ListAppointmentsHandler : IRequestHandler<ListAppointmentsQuery, Result<AppointmentPagedResultDto>>
{
    private readonly AgendaDbContext _context;

    public ListAppointmentsHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentPagedResultDto>> Handle(ListAppointmentsQuery query, CancellationToken ct)
    {
        var page = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 50 : Math.Min(query.PageSize, 200);

        var baseQuery = _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date == query.Date);

        var totalCount = await baseQuery.CountAsync(ct);

        var appointments = await baseQuery
            .OrderBy(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = appointments.Select(a => a.ToDto()).ToList();

        return Result<AppointmentPagedResultDto>.Success(
            new AppointmentPagedResultDto(dtos, totalCount, page, pageSize));
    }
}
