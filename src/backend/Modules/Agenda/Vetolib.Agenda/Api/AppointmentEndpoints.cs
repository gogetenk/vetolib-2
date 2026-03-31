using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CancelAppointmentSeries;
using Vetolib.Agenda.Application.Commands.CheckInFromQr;
using Vetolib.Agenda.Application.Commands.CreateAppointment;
using Vetolib.Agenda.Application.Commands.CreateAppointmentSeries;
using Vetolib.Agenda.Application.Commands.EditAppointment;
using Vetolib.Agenda.Application.Commands.GenerateCheckInQr;
using Vetolib.Agenda.Application.Commands.MarkWaitingRoom;
using Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;
using Vetolib.Agenda.Application.Queries.GetAppointmentById;
using Vetolib.Agenda.Application.Queries.GetAvailability;
using Vetolib.Agenda.Application.Queries.GetWaitingRoom;
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
            .RequireRateLimiting("api")
            .WithTags("Appointments");

        group.MapPost("/", CreateAppointment)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("CreateAppointment")
            .WithSummary("Create a new appointment")
            .WithDescription("Creates an appointment for a patient at a specific time slot with a veterinarian. Validates time slot availability and conflict detection.");

        group.MapGet("/", ListAppointments)
            .WithName("ListAppointments")
            .WithSummary("List appointments by date")
            .WithDescription("Returns all appointments for a given date within the current clinic.");

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateAppointmentStatus")
            .WithSummary("Update appointment status")
            .WithDescription("Directly sets the status of an appointment. Prefer the /transition endpoint for state-machine-based transitions.");

        // Action-based transition endpoint used by the frontend and BDD tests
        // Action values: CHECK_IN, START, COMPLETE, CANCEL, NO_SHOW
        group.MapPatch("/{id:guid}/transition", TransitionAppointment)
            .WithName("TransitionAppointment")
            .WithSummary("Transition appointment state")
            .WithDescription("Performs a state-machine transition on an appointment. Valid actions: CHECK_IN, START, COMPLETE, CANCEL, NO_SHOW.");

        group.MapGet("/{id:guid}", GetAppointmentById)
            .WithName("GetAppointmentById")
            .WithSummary("Get appointment by ID")
            .WithDescription("Returns the full details of a single appointment.");

        group.MapPut("/{id:guid}", EditAppointment)
            .WithName("EditAppointment")
            .WithSummary("Edit an appointment")
            .WithDescription("Updates the date, time, duration, veterinarian, or reason of an existing appointment.");

        group.MapGet("/availability", GetAvailability)
            .WithName("GetAvailability")
            .WithSummary("Get veterinarian availability")
            .WithDescription("Returns available time slots for a specific veterinarian on a given date, considering existing appointments and duration.");

        group.MapPost("/suggest-slot", SuggestSlot)
            .WithName("SuggestSlot")
            .WithSummary("Suggest an appointment slot")
            .WithDescription("Uses scheduling heuristics to suggest the best available time slot based on consultation type and preferences.");

        group.MapPost("/series", CreateAppointmentSeries)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("CreateAppointmentSeries")
            .WithSummary("Create a recurring appointment series")
            .WithDescription("Creates multiple appointments at once based on a recurrence rule (daily, weekly, biweekly, monthly) and count.");

        group.MapDelete("/series/{seriesId:guid}", CancelAppointmentSeries)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("CancelAppointmentSeries")
            .WithSummary("Cancel all future appointments in a series")
            .WithDescription("Cancels all future scheduled appointments that belong to the specified series.");

        group.MapPut("/{id:guid}/waiting-room", MarkWaitingRoom)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Receptionist"))
            .WithName("MarkWaitingRoom")
            .WithSummary("Mark patient as arrived in waiting room")
            .WithDescription("Receptionist marks a patient as arrived. Validates the appointment is Scheduled for today, sets status to WaitingRoom, and notifies the assigned vet.");

        group.MapGet("/waiting-room", GetWaitingRoomList)
            .WithName("GetWaitingRoom")
            .WithSummary("Get current waiting room patients")
            .WithDescription("Returns all patients currently in the waiting room for the clinic today, sorted by arrival time (FIFO).");

        group.MapGet("/{id:guid}/checkin-qr", GetCheckInQr)
            .WithName("GetCheckInQr")
            .WithSummary("Generate QR code payload for appointment check-in")
            .WithDescription("Returns a signed JSON payload that can be encoded into a QR code for self-service check-in at the reception desk.");

        group.MapPost("/checkin", CheckInFromQr)
            .WithName("CheckInFromQr")
            .WithSummary("Check in via QR code scan")
            .WithDescription("Accepts a signed QR payload, verifies HMAC integrity and time window (30 min before/after scheduled time), then transitions the appointment to CheckedIn.");

        // Unversioned alias (used by BDD step definitions and older clients)
        var legacyGroup = app.MapGroup("/api/appointments")
            .RequireAuthorization()
            .RequireRateLimiting("api")
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
            request.OwnerEmail,
            request.Date,
            request.StartTime,
            request.DurationMinutes,
            request.Reason);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListAppointments(
        ISender sender,
        DateOnly date,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (pageSize is < 1 or > 200) pageSize = 50;
        return (await sender.Send(new ListAppointmentsQuery(date, pageNumber, pageSize))).ToMinimalApiResult();
    }

    private static async Task<IResult> UpdateStatus(
        Guid id,
        UpdateAppointmentStatusRawRequest request,
        ISender sender)
    {
        if (!Enum.TryParse<AppointmentStatus>(request.NewStatus, ignoreCase: true, out var status))
            return Results.BadRequest(new
            {
                title = "Invalid request",
                errors = new[] { $"'{request.NewStatus}' is not a valid appointment status. Valid values: {string.Join(", ", Enum.GetNames<AppointmentStatus>())}" }
            });

        var cmd = new UpdateAppointmentStatusCommand(id, status, request.Reason);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    /// <summary>
    /// Raw request DTO that accepts status as string to enable graceful validation
    /// instead of relying on JSON enum deserialization (which would produce 500).
    /// </summary>
    internal record UpdateAppointmentStatusRawRequest(string NewStatus, string? Reason);

    private static async Task<IResult> TransitionAppointment(
        Guid id,
        TransitionAppointmentRequest request,
        ISender sender)
    {
        var newStatus = request.Action.ToUpperInvariant() switch
        {
            "WAITING_ROOM" => AppointmentStatus.WaitingRoom,
            "CHECK_IN" => AppointmentStatus.CheckedIn,
            "START"    => AppointmentStatus.InProgress,
            "COMPLETE" => AppointmentStatus.Completed,
            "CANCEL"   => AppointmentStatus.Cancelled,
            "NO_SHOW"  => AppointmentStatus.NoShow,
            _ => (AppointmentStatus?)null
        };

        if (newStatus is null)
            return Ardalis.Result.Result<AppointmentDto>
                .Error($"UNSUPPORTED_ACTION:Action '{request.Action}' is not recognized. Accepted values: WAITING_ROOM, CHECK_IN, START, COMPLETE, CANCEL, NO_SHOW")
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

    private static async Task<IResult> CreateAppointmentSeries(
        CreateAppointmentSeriesRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateAppointmentSeriesCommand(
            clinicContext.ClinicId,
            request.VeterinarianId,
            request.VeterinarianName,
            request.AnimalId,
            request.AnimalName,
            request.OwnerName,
            request.OwnerEmail,
            request.StartDate,
            request.StartTime,
            request.DurationMinutes,
            request.Reason,
            request.Frequency,
            request.Count);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> CancelAppointmentSeries(
        Guid seriesId,
        ISender sender)
    {
        return (await sender.Send(new CancelAppointmentSeriesCommand(seriesId))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetCheckInQr(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GenerateCheckInQrCommand(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> MarkWaitingRoom(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new MarkWaitingRoomCommand(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetWaitingRoomList(
        ISender sender)
    {
        return (await sender.Send(new GetWaitingRoomQuery())).ToMinimalApiResult();
    }

    private static async Task<IResult> CheckInFromQr(
        CheckInFromQrRequest request,
        ISender sender)
    {
        var cmd = new CheckInFromQrCommand(
            request.AppointmentId,
            request.PatientName,
            request.OwnerName,
            request.ScheduledTime,
            request.ClinicId,
            request.Signature);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}
