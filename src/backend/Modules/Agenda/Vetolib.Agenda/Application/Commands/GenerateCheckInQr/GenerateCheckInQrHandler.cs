using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Services;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.GenerateCheckInQr;

internal class GenerateCheckInQrHandler
    : IRequestHandler<GenerateCheckInQrCommand, Result<CheckInQrPayloadDto>>
{
    private readonly AgendaDbContext _context;
    private readonly CheckInHmacService _hmacService;

    public GenerateCheckInQrHandler(AgendaDbContext context, CheckInHmacService hmacService)
    {
        _context = context;
        _hmacService = hmacService;
    }

    public async Task<Result<CheckInQrPayloadDto>> Handle(
        GenerateCheckInQrCommand cmd, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result<CheckInQrPayloadDto>.NotFound($"Appointment {cmd.AppointmentId} not found");

        if (appointment.Status != AppointmentStatus.Scheduled)
            return Result<CheckInQrPayloadDto>.Error(
                $"INVALID_STATUS:QR code can only be generated for Scheduled appointments, current status is {appointment.Status}.");

        var scheduledTime = appointment.Date.ToDateTime(appointment.StartTime, DateTimeKind.Utc);

        var signature = _hmacService.ComputeSignature(
            appointment.Id,
            appointment.AnimalName,
            appointment.OwnerName,
            scheduledTime,
            appointment.ClinicId);

        var payload = new CheckInQrPayloadDto(
            appointment.Id,
            appointment.AnimalName,
            appointment.OwnerName,
            scheduledTime,
            appointment.ClinicId,
            signature);

        return Result<CheckInQrPayloadDto>.Success(payload);
    }
}
