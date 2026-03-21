using Ardalis.Result;
using MediatR;
using Vetolib.Notifications.Contracts.Dtos;
using Vetolib.Notifications.Contracts.Enums;

namespace Vetolib.Notifications.Application.Queries.ListReminderLogs;

internal record ListReminderLogsQuery(
    ReminderType? Type,
    int Page,
    int PageSize) : IRequest<Result<ReminderLogsPagedResult>>;

internal record ReminderLogsPagedResult(
    IReadOnlyList<ReminderLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
