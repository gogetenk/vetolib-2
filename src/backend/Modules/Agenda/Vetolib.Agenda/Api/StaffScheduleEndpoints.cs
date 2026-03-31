using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateStaffSchedule;
using Vetolib.Agenda.Application.Commands.DeleteStaffSchedule;
using Vetolib.Agenda.Application.Queries.GetMySchedule;
using Vetolib.Agenda.Application.Queries.GetStaffAvailability;
using Vetolib.Agenda.Application.Queries.ListStaffSchedules;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Api;

internal static class StaffScheduleEndpoints
{
    internal static IEndpointRouteBuilder MapStaffScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/schedule")
            .RequireAuthorization()
            .WithTags("StaffSchedule");

        group.MapGet("/", ListSchedules)
            .WithName("ListStaffSchedules")
            .WithSummary("List staff schedules for a date range")
            .WithDescription("Returns all staff schedule entries for the clinic within the specified date range.");

        group.MapGet("/me", GetMySchedule)
            .WithName("GetMySchedule")
            .WithSummary("Get current user's schedule")
            .WithDescription("Returns the authenticated user's schedule entries within the specified date range.");

        group.MapPost("/", CreateSchedule)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("CreateStaffSchedule")
            .WithSummary("Create a staff schedule entry")
            .WithDescription("Creates or updates a schedule entry for a staff member. Only clinic admins can manage schedules.");

        group.MapDelete("/{id:guid}", DeleteSchedule)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("DeleteStaffSchedule")
            .WithSummary("Remove a staff schedule entry")
            .WithDescription("Deletes a schedule entry by ID. Only clinic admins can remove schedules.");

        group.MapGet("/availability", GetAvailability)
            .WithName("GetStaffAvailability")
            .WithSummary("Get available staff for a given day")
            .WithDescription("Returns all staff members who are marked as available on the specified date.");

        return app;
    }

    private static async Task<IResult> ListSchedules(
        DateOnly from,
        DateOnly to,
        ISender sender)
    {
        return (await sender.Send(new ListStaffSchedulesQuery(from, to))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetMySchedule(
        ClaimsPrincipal user,
        ISender sender,
        DateOnly? from = null,
        DateOnly? to = null)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? user.FindFirst("sub")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var toDate = to ?? fromDate.AddDays(30);

        return (await sender.Send(new GetMyScheduleQuery(userId, fromDate, toDate))).ToMinimalApiResult();
    }

    private static async Task<IResult> CreateSchedule(
        CreateStaffScheduleRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateStaffScheduleCommand(
            clinicContext.ClinicId,
            request.UserId,
            request.UserName,
            request.Date,
            request.StartTime,
            request.EndTime,
            request.ShiftType,
            request.IsAvailable);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> DeleteSchedule(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new DeleteStaffScheduleCommand(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAvailability(
        DateOnly date,
        ISender sender)
    {
        return (await sender.Send(new GetStaffAvailabilityQuery(date))).ToMinimalApiResult();
    }
}
