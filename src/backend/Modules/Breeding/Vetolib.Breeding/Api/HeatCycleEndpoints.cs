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
            .WithName("RecordHeatCycle")
            .WithSummary("Record a heat cycle")
            .WithDescription("Records a heat cycle observation for a female patient with start date, optional end date, and notes.");

        group.MapGet("/", GetHeatCycles)
            .WithName("GetHeatCycles")
            .WithSummary("List heat cycles")
            .WithDescription("Returns all recorded heat cycles for a specific patient, ordered by most recent first.");

        group.MapGet("/prediction", PredictNextHeat)
            .WithName("PredictNextHeat")
            .WithSummary("Predict next heat cycle")
            .WithDescription("Calculates the predicted date of the next heat cycle based on historical cycle data for the patient.");

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
