using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Notifications.Contracts.Dtos;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Application.Queries.ListReminderLogs;

internal class ListReminderLogsHandler : IRequestHandler<ListReminderLogsQuery, Result<ReminderLogsPagedResult>>
{
    private readonly NotificationsDbContext _dbContext;
    private readonly IClinicContext _clinicContext;

    public ListReminderLogsHandler(NotificationsDbContext dbContext, IClinicContext clinicContext)
    {
        _dbContext = dbContext;
        _clinicContext = clinicContext;
    }

    public async Task<Result<ReminderLogsPagedResult>> Handle(ListReminderLogsQuery request, CancellationToken ct)
    {
        var query = _dbContext.ReminderLogs
            .IgnoreQueryFilters()
            .Where(r => r.ClinicId == _clinicContext.ClinicId);

        if (request.Type.HasValue)
            query = query.Where(r => r.ReminderType == request.Type.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.SentAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReminderLogDto(
                r.Id,
                r.AppointmentId,
                r.PatientId,
                r.ReminderType,
                r.SentAt,
                r.Channel,
                r.DeliveryStatus,
                r.RecipientEmail))
            .ToListAsync(ct);

        return Result<ReminderLogsPagedResult>.Success(
            new ReminderLogsPagedResult(items, totalCount, request.Page, request.PageSize));
    }
}
