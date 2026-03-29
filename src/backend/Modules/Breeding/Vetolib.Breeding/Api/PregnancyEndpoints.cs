using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Breeding.Application.Commands.CompleteCheck;
using Vetolib.Breeding.Application.Commands.CreatePregnancy;
using Vetolib.Breeding.Application.Commands.RecordDelivery;
using Vetolib.Breeding.Application.Commands.RecordLoss;
using Vetolib.Breeding.Application.Commands.ScheduleCheck;
using Vetolib.Breeding.Application.Queries.GetActivePregnancies;
using Vetolib.Breeding.Application.Queries.GetPregnanciesByPatient;
using Vetolib.Breeding.Application.Queries.GetPregnancyById;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Api;

internal static class PregnancyEndpoints
{
    internal static IEndpointRouteBuilder MapPregnancyApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/breeding/pregnancies")
            .RequireAuthorization()
            .WithTags("Breeding - Pregnancies");

        group.MapPost("/", Create).WithName("CreatePregnancy");
        group.MapGet("/{id:guid}", GetById).WithName("GetPregnancyById");
        group.MapGet("/by-patient/{patientId:guid}", GetByPatient).WithName("GetPregnanciesByPatient");
        group.MapGet("/active", GetActive).WithName("GetActivePregnancies");
        group.MapPut("/{id:guid}/delivery", RecordDelivery).WithName("RecordDelivery");
        group.MapPut("/{id:guid}/loss", RecordLoss).WithName("RecordLoss");
        group.MapPost("/{id:guid}/checks", ScheduleCheck).WithName("SchedulePregnancyCheck");
        group.MapPut("/checks/{checkId:guid}/complete", CompleteCheck).WithName("CompletePregnancyCheck");

        return app;
    }

    private static async Task<IResult> Create(
        CreatePregnancyRequest req,
        ISender sender)
        => (await sender.Send(new CreatePregnancyCommand(
            req.PatientId,
            req.FatherPatientId,
            req.MatingDate,
            req.MatingMethod,
            // PatientSex and PatientSpecies will be resolved by middleware or enriched before reaching the handler.
            // For now, they must be provided. In production, an endpoint filter would fetch them from MedicalRecords.
            "Female",
            "Dog",
            req.Notes)))
            .ToMinimalApiResult();

    private static async Task<IResult> GetById(Guid id, ISender sender)
        => (await sender.Send(new GetPregnancyByIdQuery(id))).ToMinimalApiResult();

    private static async Task<IResult> GetByPatient(Guid patientId, ISender sender)
        => (await sender.Send(new GetPregnanciesByPatientQuery(patientId))).ToMinimalApiResult();

    private static async Task<IResult> GetActive(ISender sender)
        => (await sender.Send(new GetActivePregnanciesQuery())).ToMinimalApiResult();

    private static async Task<IResult> RecordDelivery(
        Guid id,
        RecordDeliveryRequest req,
        ISender sender)
        => (await sender.Send(new RecordDeliveryCommand(
            id, req.DeliveryDate, req.Outcome, req.OffspringCount, req.Notes)))
            .ToMinimalApiResult();

    private static async Task<IResult> RecordLoss(
        Guid id,
        RecordDeliveryRequest req,
        ISender sender)
        => (await sender.Send(new RecordLossCommand(
            id, req.DeliveryDate, req.Outcome, req.Notes)))
            .ToMinimalApiResult();

    private static async Task<IResult> ScheduleCheck(
        Guid id,
        ScheduleCheckRequest req,
        ISender sender)
        => (await sender.Send(new ScheduleCheckCommand(
            id, req.ScheduledDate, req.CheckType, req.Note)))
            .ToMinimalApiResult();

    private static async Task<IResult> CompleteCheck(
        Guid checkId,
        CompleteCheckRequest req,
        ISender sender)
        => (await sender.Send(new CompleteCheckCommand(checkId, req.Result)))
            .ToMinimalApiResult();
}
