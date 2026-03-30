using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Commands.CreateOwner;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class OwnerEndpoints
{
    internal static IEndpointRouteBuilder MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/owners")
            .RequireAuthorization()
            .WithTags("Owners");

        group.MapPost("/", CreateOwner)
            .WithName("CreateOwner")
            .WithSummary("Create a new owner")
            .WithDescription("Registers a new pet owner with contact details. Requires Vet, Admin, or Receptionist role.");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateOwner(
        CreateOwnerRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        // Only VET, Admin, and Receptionist can create owners
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role is not ("Vet" or "Admin" or "Receptionist"))
            return Ardalis.Result.Result<OwnerDto>.Forbidden().ToMinimalApiResult();

        var cmd = new CreateOwnerCommand(
            clinicContext.ClinicId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}
