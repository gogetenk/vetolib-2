using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.GetMessagingHours;

internal record GetMessagingHoursQuery : IRequest<Result<IReadOnlyList<MessagingHoursDto>>>;
