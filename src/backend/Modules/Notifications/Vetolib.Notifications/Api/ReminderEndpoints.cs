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

        group.MapGet("/config", GetConfig).WithName("GetReminderConfig")
            .WithSummary("Get reminder configuration")
            .WithDescription("Returns the current reminder settings including enabled types and lead times.");
        group.MapPut("/config", UpdateConfig).WithName("UpdateReminderConfig")
            .WithSummary("Update reminder configuration")
            .WithDescription("Updates reminder settings such as appointment, vaccination, and follow-up reminder toggles and lead times.");
        group.MapGet("/logs", GetLogs).WithName("ListReminderLogs")
            .WithSummary("List reminder logs")
            .WithDescription("Returns a paginated list of sent reminder logs, optionally filtered by reminder type.");

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
            dto.VaccinationDueLeadTimeDays,
            dto.PreferredReminderChannel))).ToMinimalApiResult();

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
