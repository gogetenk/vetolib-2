using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Breeding.Application.Commands.AddOffspringToLitter;
using Vetolib.Breeding.Application.Commands.CreateLitter;
using Vetolib.Breeding.Application.Queries.GetLitterById;
using Vetolib.Breeding.Application.Queries.GetLittersByMother;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Api;

internal static class LitterEndpoints
{
    internal static IEndpointRouteBuilder MapLitterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/litters")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Litters");

        group.MapPost("/", Create).WithName("CreateLitter")
            .WithSummary("Record a new litter")
            .WithDescription("Creates a litter record for a mother patient with birth date, counts, and optional father reference.");
        group.MapGet("/{id:guid}", GetById).WithName("GetLitterById")
            .WithSummary("Get litter by ID")
            .WithDescription("Returns the full details of a litter including offspring list.");
        group.MapPost("/{id:guid}/offspring", AddOffspring).WithName("AddOffspringToLitter")
            .WithSummary("Add offspring to a litter")
            .WithDescription("Links an existing patient record as an offspring of the litter with a birth order.");

        // Patient-scoped litter listing
        app.MapGet("/api/v1/patients/{id:guid}/litters", GetByMother)
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Litters")
            .WithName("GetLittersByMother")
            .WithSummary("Get litters by mother")
            .WithDescription("Returns all litters for a specific mother patient.");

        return app;
    }

    private static async Task<IResult> Create(
        CreateLitterRequest req,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateLitterCommand(
            clinicContext.ClinicId,
            req.MotherPatientId,
            req.FatherPatientId,
            req.ExternalFatherName,
            req.BirthDate,
            req.BornCount,
            req.AliveCount,
            req.Notes);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetById(Guid id, ISender sender)
        => (await sender.Send(new GetLitterByIdQuery(id))).ToMinimalApiResult();

    private static async Task<IResult> AddOffspring(
        Guid id,
        AddOffspringToLitterRequest req,
        ISender sender)
        => (await sender.Send(new AddOffspringToLitterCommand(id, req.PatientId, req.BirthOrder)))
            .ToMinimalApiResult();

    private static async Task<IResult> GetByMother(Guid id, ISender sender)
        => (await sender.Send(new GetLittersByMotherQuery(id))).ToMinimalApiResult();
}
