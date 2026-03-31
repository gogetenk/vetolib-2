using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Services;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CheckInFromQr;

internal class CheckInFromQrHandler
    : IRequestHandler<CheckInFromQrCommand, Result<AppointmentDto>>
{
    private readonly AgendaDbContext _context;
    private readonly CheckInHmacService _hmacService;
    private readonly TimeProvider _timeProvider;

    public CheckInFromQrHandler(
        AgendaDbContext context,
        CheckInHmacService hmacService,
        TimeProvider timeProvider)
    {
        _context = context;
        _hmacService = hmacService;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AppointmentDto>> Handle(
        CheckInFromQrCommand cmd, CancellationToken ct)
    {
        // 1. Verify HMAC and time window
        var verifyResult = _hmacService.VerifyAndValidateTimeWindow(
            cmd.AppointmentId,
            cmd.PatientName,
            cmd.OwnerName,
            cmd.ScheduledTime,
            cmd.ClinicId,
            cmd.Signature,
            _timeProvider.GetUtcNow().UtcDateTime);

        if (!verifyResult.IsSuccess)
            return Result<AppointmentDto>.Error(string.Join("; ", verifyResult.Errors));

        // 2. Load appointment
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result<AppointmentDto>.NotFound($"Appointment {cmd.AppointmentId} not found");

        // 3. Transition to CheckedIn
        var checkInResult = appointment.CheckIn();
        if (!checkInResult.IsSuccess)
            return Result<AppointmentDto>.Error(string.Join("; ", checkInResult.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointment.ToDto());
    }
}
