using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateAppointment;
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

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateAppointment(
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

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListAppointments(
        DateOnly date,
        ISender sender)
    {
        return (await sender.Send(new ListAppointmentsQuery(date))).ToMinimalApiResult();
    }
}
