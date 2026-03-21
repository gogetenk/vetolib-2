using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Notifications.Application.Commands.UpdateReminderConfig;
using Vetolib.Notifications.Application.Queries.GetReminderConfig;
using Vetolib.Notifications.Application.Queries.ListReminderLogs;
using Vetolib.Notifications.Contracts.Dtos;
using Vetolib.Notifications.Contracts.Enums;

namespace Vetolib.Notifications.Api;

internal static class ReminderEndpoints
{
    internal static IEndpointRouteBuilder MapReminderApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications/reminders")
            .RequireAuthorization("VetOrAdmin")
            .WithTags("Reminders");

        group.MapGet("/config", GetConfig).WithName("GetReminderConfig");
        group.MapPut("/config", UpdateConfig).WithName("UpdateReminderConfig");
        group.MapGet("/logs", GetLogs).WithName("ListReminderLogs");

        return app;
    }

    private static async Task<IResult> GetConfig(ISender sender)
        => (await sender.Send(new GetReminderConfigQuery())).ToMinimalApiResult();

    private static async Task<IResult> UpdateConfig(
        ReminderConfigDto dto,
        ISender sender)
        => (await sender.Send(new UpdateReminderConfigCommand(
            dto.Appointment24hEnabled,
            dto.VaccinationDueEnabled,
            dto.FollowUpEnabled,
            dto.Appointment24hLeadTimeHours,
            dto.VaccinationDueLeadTimeDays))).ToMinimalApiResult();

    private static async Task<IResult> GetLogs(
        ReminderType? type,
        int? page,
        int? pageSize,
        ISender sender)
        => (await sender.Send(new ListReminderLogsQuery(
            type,
            page ?? 1,
            pageSize ?? 20))).ToMinimalApiResult();
}
