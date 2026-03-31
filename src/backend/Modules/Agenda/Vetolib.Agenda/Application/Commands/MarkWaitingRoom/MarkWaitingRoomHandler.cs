using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.MarkWaitingRoom;

internal class MarkWaitingRoomHandler
    : IRequestHandler<MarkWaitingRoomCommand, Result<AppointmentDto>>
{
    private readonly AgendaDbContext _context;

    public MarkWaitingRoomHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(
        MarkWaitingRoomCommand cmd, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result<AppointmentDto>.NotFound($"Appointment {cmd.AppointmentId} not found");

        if (appointment.Date != DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<AppointmentDto>.Error("INVALID_DATE:Appointment must be scheduled for today");

        var transitionResult = appointment.MarkWaitingRoom();
        if (!transitionResult.IsSuccess)
            return Result<AppointmentDto>.Error(string.Join("; ", transitionResult.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointment.ToDto());
    }
}
