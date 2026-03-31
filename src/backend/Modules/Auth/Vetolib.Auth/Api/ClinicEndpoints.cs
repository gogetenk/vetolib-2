using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.RegisterClinic;
using Vetolib.Auth.Application.Queries.SearchClinics;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class ClinicEndpoints
{
    internal static IEndpointRouteBuilder MapClinicApiEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/v1/clinics")
            .WithTags("Clinics");

        publicGroup.MapPost("/register", RegisterClinic)
            .WithName("RegisterClinic")
            .AllowAnonymous()
            .RequireRateLimiting("signup")
            .WithSummary("Register a new clinic")
            .WithDescription("Creates a new clinic with an admin user account. This is the entry point for new clinic sign-ups. Rate limited to prevent abuse.")
            .Produces<RegisterClinicResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status429TooManyRequests);

        publicGroup.MapGet("/search", SearchClinics)
            .WithName("SearchClinics")
            .AllowAnonymous()
            .WithSummary("Search the public clinic directory")
            .WithDescription("Search for veterinary clinics by name, city, or supported species. Returns paginated results. No authentication required — this is a public directory endpoint.")
            .Produces<ClinicSearchPagedResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> RegisterClinic(
        RegisterClinicRequest request,
        ISender sender)
    {
        var result = await sender.Send(new RegisterClinicCommand(
            request.ClinicName,
            request.Email,
            request.Password,
            request.Phone,
            request.Country,
            request.ReferralCode));

        if (!result.IsSuccess)
            return result.ToMinimalApiResult();

        // Return 201 Created instead of default 200
        return Results.Created($"/api/v1/clinics/{result.Value.ClinicId}", result.Value);
    }

    private static async Task<IResult> SearchClinics(
        ISender sender,
        string? name = null,
        string? city = null,
        string? species = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new SearchClinicsQuery(name, city, species, page, pageSize));
        return result.ToMinimalApiResult();
    }
}
