using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.GenerateCheckInQr;

internal record GenerateCheckInQrCommand(Guid AppointmentId) : IRequest<Result<CheckInQrPayloadDto>>;
