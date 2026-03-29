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
            .WithTags("Litters");

        group.MapPost("/", Create).WithName("CreateLitter");
        group.MapGet("/{id:guid}", GetById).WithName("GetLitterById");
        group.MapPost("/{id:guid}/offspring", AddOffspring).WithName("AddOffspringToLitter");

        // Patient-scoped litter listing
        app.MapGet("/api/v1/patients/{id:guid}/litters", GetByMother)
            .RequireAuthorization()
            .WithTags("Litters")
            .WithName("GetLittersByMother");

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
