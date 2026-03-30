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
            .WithTags("Lineage");

        group.MapPut("/lineage", SetLineage).WithName("SetLineage");
        group.MapGet("/lineage", GetLineage).WithName("GetLineage");
        group.MapGet("/pedigree", GetPedigree).WithName("GetPedigree");
        group.MapGet("/descendants", GetDescendants).WithName("GetDescendants");

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
