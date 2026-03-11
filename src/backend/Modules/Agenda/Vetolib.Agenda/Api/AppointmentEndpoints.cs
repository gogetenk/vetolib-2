using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateAppointment;
using Vetolib.Agenda.Application.Commands.EditAppointment;
using Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;
using Vetolib.Agenda.Application.Queries.GetAppointmentById;
using Vetolib.Agenda.Application.Queries.GetAvailability;
using Vetolib.Agenda.Application.Queries.ListAppointments;
using Vetolib.Agenda.Application.Queries.SuggestSlot;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Api;

internal static class AppointmentEndpoints
{
    internal static IEndpointRouteBuilder MapAppointmentApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Versioned group (canonical)
        var group = app.MapGroup("/api/v1/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        group.MapPost("/", CreateAppointment)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("CreateAppointment");

        group.MapGet("/", ListAppointments)
            .WithName("ListAppointments");

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateAppointmentStatus");

        // Action-based transition endpoint used by the frontend and BDD tests
        // Action values: CHECK_IN, START, COMPLETE, CANCEL, NO_SHOW
        group.MapPatch("/{id:guid}/transition", TransitionAppointment)
            .WithName("TransitionAppointment");

        group.MapGet("/{id:guid}", GetAppointmentById)
            .WithName("GetAppointmentById");

        group.MapPut("/{id:guid}", EditAppointment)
            .WithName("EditAppointment");

        group.MapGet("/availability", GetAvailability)
            .WithName("GetAvailability");

        group.MapPost("/suggest-slot", SuggestSlot)
            .WithName("SuggestSlot");

        // Unversioned alias (used by BDD step definitions and older clients)
        var legacyGroup = app.MapGroup("/api/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        legacyGroup.MapPost("/", CreateAppointment)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"));
        legacyGroup.MapGet("/", ListAppointments);
        legacyGroup.MapPatch("/{id:guid}/transition", TransitionAppointment);
        legacyGroup.MapGet("/availability", GetAvailability);
        legacyGroup.MapGet("/{id:guid}", GetAppointmentById);
        legacyGroup.MapPut("/{id:guid}", EditAppointment);

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

    private static async Task<IResult> TransitionAppointment(
        Guid id,
        TransitionAppointmentRequest request,
        ISender sender)
    {
        var newStatus = request.Action.ToUpperInvariant() switch
        {
            "CHECK_IN" => AppointmentStatus.CheckedIn,
            "START"    => AppointmentStatus.InProgress,
            "COMPLETE" => AppointmentStatus.Completed,
            "CANCEL"   => AppointmentStatus.Cancelled,
            "NO_SHOW"  => AppointmentStatus.NoShow,
            _ => (AppointmentStatus?)null
        };

        if (newStatus is null)
            return Ardalis.Result.Result<AppointmentDto>
                .Error($"UNSUPPORTED_ACTION:Action '{request.Action}' non reconnue. Valeurs acceptees: CHECK_IN, START, COMPLETE, CANCEL, NO_SHOW")
                .ToMinimalApiResult();

        var cmd = new UpdateAppointmentStatusCommand(id, newStatus.Value, request.Reason);
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

    private static async Task<IResult> SuggestSlot(
        SuggestSlotRequest request,
        ISender sender)
    {
        var query = new SuggestSlotQuery(
            request.ConsultationType,
            request.PreferredDate,
            request.PreferredTime,
            request.PreferredVeterinarianId,
            request.DurationMinutes);

        return (await sender.Send(query)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAppointmentById(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetAppointmentByIdQuery(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> EditAppointment(
        Guid id,
        UpdateAppointmentRequest request,
        ISender sender)
    {
        var cmd = new EditAppointmentCommand(
            AppointmentId: id,
            Date: request.Date,
            StartTime: request.StartTime,
            DurationMinutes: request.DurationMinutes,
            VeterinarianId: request.VeterinarianId,
            VeterinarianName: request.VeterinarianName,
            Reason: request.Reason,
            Notes: request.Notes);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}
