using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateAppointment;
using Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;
using Vetolib.Agenda.Application.Queries.GetAvailability;
using Vetolib.Agenda.Application.Queries.ListAppointments;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Api;

internal static class AppointmentEndpoints
{
    internal static IEndpointRouteBuilder MapAppointmentApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        group.MapPost("/", CreateAppointment)
            .WithName("CreateAppointment");

        group.MapGet("/", ListAppointments)
            .WithName("ListAppointments");

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateAppointmentStatus");

        group.MapGet("/availability", GetAvailability)
            .WithName("GetAvailability");

        return app;
    }

    private static async Task<IResult> CreateAppointment(
        CreateAppointmentRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateAppointmentCommand(
            clinicContext.ClinicId,
            request.VeterinarianId,
            request.VeterinarianName,
            request.AnimalId,
            request.AnimalName,
            request.OwnerName,
            request.Date,
            request.StartTime,
            request.DurationMinutes,
            request.Reason);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListAppointments(
        DateOnly date,
        ISender sender)
    {
        return (await sender.Send(new ListAppointmentsQuery(date))).ToMinimalApiResult();
    }

    private static async Task<IResult> UpdateStatus(
        Guid id,
        UpdateAppointmentStatusRequest request,
        ISender sender)
    {
        var cmd = new UpdateAppointmentStatusCommand(id, request.NewStatus, request.Reason);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAvailability(
        Guid veterinarianId,
        DateOnly date,
        int durationMinutes,
        ISender sender)
    {
        return (await sender.Send(new GetAvailabilityQuery(veterinarianId, date, durationMinutes))).ToMinimalApiResult();
    }
}
