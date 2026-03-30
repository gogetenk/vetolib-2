using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CheckInFromQr;

internal record CheckInFromQrCommand(
    Guid AppointmentId,
    string PatientName,
    string OwnerName,
    DateTime ScheduledTime,
    Guid ClinicId,
    string Signature) : IRequest<Result<AppointmentDto>>;
