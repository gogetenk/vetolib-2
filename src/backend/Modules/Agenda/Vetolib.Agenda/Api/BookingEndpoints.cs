using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Api;

/// <summary>
/// Public endpoints for the owner self-booking portal.
/// These endpoints require NO authentication — they are accessed before the owner has a magic link token.
/// Rate limited via the "api" policy (100 req/min per IP).
/// </summary>
internal static class BookingEndpoints
{
    internal static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/portal/booking")
            .WithTags("OwnerBooking")
            .RequireRateLimiting("api");

        // GET /api/v1/portal/booking/veterinarians?clinicId={clinicId}
        // Public endpoint — allows owner portal to list available vets without prior authentication.
        // The clinicId is provided as a query parameter (resolved from the clinic URL/slug on the frontend).
        group.MapGet("/veterinarians", GetVeterinarians)
            .WithName("ListBookingVeterinarians")
            .Produces<List<ClinicVeterinarianDto>>();

        return app;
    }

    private static async Task<IResult> GetVeterinarians(
        Guid clinicId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ListClinicVeterinariansQuery(clinicId), ct);
        return result.ToMinimalApiResult();
    }
}
