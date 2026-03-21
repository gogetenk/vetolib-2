using Ardalis.Result;
using MediatR;
using Vetolib.Notifications.Contracts.Dtos;

namespace Vetolib.Notifications.Application.Queries.GetReminderConfig;

internal record GetReminderConfigQuery : IRequest<Result<ReminderConfigDto>>;
