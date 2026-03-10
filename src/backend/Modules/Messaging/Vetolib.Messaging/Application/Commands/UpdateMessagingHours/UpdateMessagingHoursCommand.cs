using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.UpdateMessagingHours;

internal record UpdateMessagingHoursCommand(
    IReadOnlyList<MessagingHoursDayRequest> Days
) : IRequest<Result<IReadOnlyList<MessagingHoursDto>>>;
