using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.ConvertAlertToAppointment;

internal record ConvertAlertToAppointmentCommand(Guid AlertId) : IRequest<Result<AppointmentPreFillDto>>;
