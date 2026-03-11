using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateBookingAppointment;
using Vetolib.Agenda.Application.Queries.GetOwnerAppointmentById;
using Vetolib.Agenda.Application.Queries.ListOwnerAppointments;
using Vetolib.Agenda.Application.Queries.ListPublicConsultationTypes;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Api;

/// <summary>
/// Endpoints for the owner self-booking portal.
///
/// Public (no auth, rate limited):
///   GET /api/v1/portal/booking/consultation-types?clinicId=... — list active types for clinic
///
/// MagicLink auth (via AgendaBookingPortalFilter):
///   POST /api/v1/portal/booking/appointments  — create appointment (BookingSource.OwnerPortal)
///   GET  /api/v1/portal/booking/appointments  — list owner's appointments
///   GET  /api/v1/portal/booking/appointments/{id} — appointment detail (ownership verified)
/// </summary>
internal static class BookingEndpoints
{
    internal static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Public group — no auth, rate limited ──────────────────────────────────
        var publicGroup = app.MapGroup("/api/v1/portal/booking")
            .WithTags("OwnerBooking")
            .RequireRateLimiting("api");

        // GET /api/v1/portal/booking/veterinarians?clinicId={clinicId}
        // Already-existing public endpoint — lists available vets for a clinic.
        publicGroup.MapGet("/veterinarians", GetVeterinarians)
            .WithName("ListBookingVeterinarians")
            .Produces<List<ClinicVeterinarianDto>>();

        // GET /api/v1/portal/booking/consultation-types?clinicId={clinicId}
        // Public — owner portal calls this before authentication to display available types.
        publicGroup.MapGet("/consultation-types", ListPublicConsultationTypes)
            .WithName("ListPublicConsultationTypes")
            .Produces<List<ConsultationTypeDto>>();

        // ── MagicLink auth group ──────────────────────────────────────────────────
        var portalGroup = app.MapGroup("/api/v1/portal/booking")
            .WithTags("OwnerBooking")
            .RequireRateLimiting("api")
            .AddEndpointFilter<AgendaBookingPortalFilter>();

        portalGroup.MapPost("/appointments", CreateBookingAppointment)
            .WithName("CreateBookingAppointment")
            .Produces<AppointmentDto>(StatusCodes.Status201Created);

        portalGroup.MapGet("/appointments", ListOwnerAppointments)
            .WithName("ListOwnerAppointments")
            .Produces<List<AppointmentDto>>();

        portalGroup.MapGet("/appointments/{id:guid}", GetOwnerAppointmentById)
            .WithName("GetOwnerAppointmentById")
            .Produces<AppointmentDto>();

        return app;
    }

    // ── Handlers ─────────────────────────────────────────────────────────────────

    private static async Task<IResult> GetVeterinarians(
        Guid clinicId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ListClinicVeterinariansQuery(clinicId), ct);
        return result.ToMinimalApiResult();
    }

    private static async Task<IResult> ListPublicConsultationTypes(
        Guid clinicId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ListPublicConsultationTypesQuery(clinicId), ct);
        return result.ToMinimalApiResult();
    }

    private static async Task<IResult> CreateBookingAppointment(
        BookingAppointmentRequest request,
        IAgendaPortalContext portal,
        ISender sender,
        CancellationToken ct)
    {
        var cmd = new CreateBookingAppointmentCommand(
            ClinicId: portal.ClinicId,
            OwnerId: portal.OwnerId,
            VeterinarianId: request.VeterinarianId,
            VeterinarianName: request.VeterinarianName,
            AnimalId: request.AnimalId,
            AnimalName: request.AnimalName,
            OwnerName: request.OwnerName,
            Date: request.Date,
            StartTime: request.StartTime,
            DurationMinutes: request.DurationMinutes,
            Reason: request.Reason);

        var result = await sender.Send(cmd, ct);
        return result.ToMinimalApiResult();
    }

    private static async Task<IResult> ListOwnerAppointments(
        IAgendaPortalContext portal,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ListOwnerAppointmentsQuery(portal.OwnerId, portal.ClinicId), ct);
        return result.ToMinimalApiResult();
    }

    private static async Task<IResult> GetOwnerAppointmentById(
        Guid id,
        IAgendaPortalContext portal,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetOwnerAppointmentByIdQuery(id, portal.OwnerId, portal.ClinicId), ct);
        return result.ToMinimalApiResult();
    }
}

/// <summary>Request DTO for the owner portal booking endpoint.</summary>
internal record BookingAppointmentRequest(
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Reason);
