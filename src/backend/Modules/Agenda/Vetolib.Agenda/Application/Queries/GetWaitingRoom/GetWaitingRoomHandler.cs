using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetWaitingRoom;

internal class GetWaitingRoomHandler
    : IRequestHandler<GetWaitingRoomQuery, Result<List<WaitingRoomAppointmentDto>>>
{
    private readonly AgendaDbContext _context;

    public GetWaitingRoomHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WaitingRoomAppointmentDto>>> Handle(
        GetWaitingRoomQuery query, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var waitingRoomAppointments = await _context.Appointments
            .Where(a => a.Date == today)
            .Where(a => a.Status == AppointmentStatus.WaitingRoom)
            .OrderBy(a => a.WaitingRoomAt)
            .Select(a => new WaitingRoomAppointmentDto(
                a.Id,
                a.AnimalName,
                a.OwnerName,
                a.StartTime,
                a.VeterinarianName,
                a.WaitingRoomAt!.Value,
                (int)(now - a.WaitingRoomAt!.Value).TotalMinutes))
            .ToListAsync(ct);

        return Result<List<WaitingRoomAppointmentDto>>.Success(waitingRoomAppointments);
    }
}
