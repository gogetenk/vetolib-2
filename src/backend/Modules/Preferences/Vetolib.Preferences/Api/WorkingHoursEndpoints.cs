using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Preferences.Application.Commands.UpsertWorkingHours;
using Vetolib.Preferences.Application.Queries.GetWorkingHours;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Api;

internal static class WorkingHoursEndpoints
{
    internal static IEndpointRouteBuilder MapWorkingHoursEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/preferences/working-hours")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("WorkingHours");

        group.MapGet("/", GetWorkingHours)
            .WithName("GetWorkingHours")
            .WithSummary("Get clinic working hours")
            .WithDescription("Returns the working hours for all 7 days of the week for the current clinic. If none are configured, UAE defaults are returned (Sun-Thu 8-18, Fri 8-12, Sat closed).");

        group.MapPut("/", UpsertWorkingHours)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("UpsertWorkingHours")
            .WithSummary("Set clinic working hours")
            .WithDescription("Creates or updates working hours for the specified days. Only Admin users can modify working hours. Supports split shifts via BreakStartTime/BreakEndTime.");

        return app;
    }

    private static async Task<IResult> GetWorkingHours(ISender sender)
    {
        return (await sender.Send(new GetWorkingHoursQuery())).ToMinimalApiResult();
    }

    private static async Task<IResult> UpsertWorkingHours(
        UpsertWorkingHoursRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new UpsertWorkingHoursCommand(
            clinicContext.ClinicId,
            request.Days.Select(d => new WorkingHoursItemRequest(
                d.DayOfWeek, d.IsOpen, d.OpenTime, d.CloseTime,
                d.BreakStartTime, d.BreakEndTime)).ToList());

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}

internal record UpsertWorkingHoursRequest(IReadOnlyList<UpsertWorkingHoursDayRequest> Days);

internal record UpsertWorkingHoursDayRequest(
    DayOfWeek DayOfWeek,
    bool IsOpen,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    TimeOnly? BreakStartTime,
    TimeOnly? BreakEndTime);
