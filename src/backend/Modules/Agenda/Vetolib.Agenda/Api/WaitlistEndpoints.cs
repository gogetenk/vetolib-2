using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.AddToWaitlist;
using Vetolib.Agenda.Application.Commands.RemoveFromWaitlist;
using Vetolib.Agenda.Application.Queries.ListWaitlistEntries;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Api;

internal static class WaitlistEndpoints
{
    internal static IEndpointRouteBuilder MapWaitlistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/waitlist")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Waitlist");

        group.MapPost("/", AddToWaitlist)
            .WithName("AddToWaitlist")
            .WithSummary("Add a patient to the waiting list")
            .WithDescription("Adds a patient to the clinic's waiting list. When an appointment slot becomes available through cancellation, matching waitlist entries are notified.");

        group.MapGet("/", ListWaitlistEntries)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("ListWaitlistEntries")
            .WithSummary("List waitlist entries for the clinic")
            .WithDescription("Returns paginated waitlist entries for the current clinic, ordered by creation date. Supports page and pageSize query parameters.");

        group.MapDelete("/{id:guid}", RemoveFromWaitlist)
            .WithName("RemoveFromWaitlist")
            .WithSummary("Remove a waitlist entry")
            .WithDescription("Removes a patient from the waiting list.");

        return app;
    }

    private static async Task<IResult> AddToWaitlist(
        CreateWaitlistEntryRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new AddToWaitlistCommand(
            clinicContext.ClinicId,
            request.PatientId,
            request.OwnerName,
            request.OwnerPhone,
            request.OwnerEmail,
            request.PreferredDate,
            request.PreferredTimeSlot,
            request.VetPreference,
            request.Reason);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListWaitlistEntries(
        ISender sender,
        int page = 1,
        int pageSize = 20)
    {
        return (await sender.Send(new ListWaitlistEntriesQuery(page, pageSize))).ToMinimalApiResult();
    }

    private static async Task<IResult> RemoveFromWaitlist(Guid id, ISender sender)
    {
        return (await sender.Send(new RemoveFromWaitlistCommand(id))).ToMinimalApiResult();
    }
}
