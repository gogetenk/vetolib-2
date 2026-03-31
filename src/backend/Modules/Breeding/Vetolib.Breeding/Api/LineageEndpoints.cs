using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Breeding.Application.Commands.SetLineage;
using Vetolib.Breeding.Application.Queries.GetDescendants;
using Vetolib.Breeding.Application.Queries.GetLineage;
using Vetolib.Breeding.Application.Queries.GetPedigree;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Api;

internal static class LineageEndpoints
{
    internal static IEndpointRouteBuilder MapLineageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients/{id:guid}")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Lineage");

        group.MapPut("/lineage", SetLineage).WithName("SetLineage")
            .WithSummary("Set patient lineage")
            .WithDescription("Sets or updates the mother, father, and registry information for a patient's lineage.");
        group.MapGet("/lineage", GetLineage).WithName("GetLineage")
            .WithSummary("Get patient lineage")
            .WithDescription("Returns the direct parent information (mother and father) for a patient.");
        group.MapGet("/pedigree", GetPedigree).WithName("GetPedigree")
            .WithSummary("Get patient pedigree tree")
            .WithDescription("Returns the multi-generational pedigree tree for a patient. Defaults to 3 generations.");
        group.MapGet("/descendants", GetDescendants).WithName("GetDescendants")
            .WithSummary("Get patient descendants")
            .WithDescription("Returns all known descendants (children, grandchildren, etc.) of a patient.");

        return app;
    }

    private static async Task<IResult> SetLineage(
        Guid id,
        SetLineageRequest req,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new SetLineageCommand(
            clinicContext.ClinicId,
            id,
            req.MotherPatientId,
            req.FatherPatientId,
            req.RegistryNumber,
            req.RegistryType);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetLineage(Guid id, ISender sender)
        => (await sender.Send(new GetLineageQuery(id))).ToMinimalApiResult();

    private static async Task<IResult> GetPedigree(Guid id, int? generations, ISender sender)
        => (await sender.Send(new GetPedigreeQuery(id, generations ?? 3))).ToMinimalApiResult();

    private static async Task<IResult> GetDescendants(Guid id, ISender sender)
        => (await sender.Send(new GetDescendantsQuery(id))).ToMinimalApiResult();
}
