using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.RegisterClinic;
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
            .Produces<RegisterClinicResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status429TooManyRequests);

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
            request.Country));

        if (!result.IsSuccess)
            return result.ToMinimalApiResult();

        // Return 201 Created instead of default 200
        return Results.Created($"/api/v1/clinics/{result.Value.ClinicId}", result.Value);
    }
}
