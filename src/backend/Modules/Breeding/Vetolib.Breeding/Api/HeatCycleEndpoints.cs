using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Breeding.Application.Commands.RecordHeatCycle;
using Vetolib.Breeding.Application.Queries.GetHeatCycles;
using Vetolib.Breeding.Application.Queries.PredictNextHeat;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Api;

internal static class HeatCycleEndpoints
{
    internal static IEndpointRouteBuilder MapHeatCycleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients/{patientId:guid}/heat-cycles")
            .RequireAuthorization("VetOrAdmin")
            .WithTags("HeatCycles");

        group.MapPost("/", RecordHeatCycle)
            .WithName("RecordHeatCycle");

        group.MapGet("/", GetHeatCycles)
            .WithName("GetHeatCycles");

        group.MapGet("/prediction", PredictNextHeat)
            .WithName("PredictNextHeat");

        return app;
    }

    private static async Task<IResult> RecordHeatCycle(
        Guid patientId,
        RecordHeatCycleRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new RecordHeatCycleCommand(
            clinicContext.ClinicId,
            patientId,
            request.StartDate,
            request.EndDate,
            request.Notes);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetHeatCycles(
        Guid patientId,
        ISender sender)
    {
        return (await sender.Send(new GetHeatCyclesQuery(patientId))).ToMinimalApiResult();
    }

    private static async Task<IResult> PredictNextHeat(
        Guid patientId,
        ISender sender)
    {
        return (await sender.Send(new PredictNextHeatQuery(patientId))).ToMinimalApiResult();
    }
}
