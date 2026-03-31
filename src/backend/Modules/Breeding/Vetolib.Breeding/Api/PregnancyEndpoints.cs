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
            .RequireRateLimiting("api")
            .WithTags("Breeding - Pregnancies");

        group.MapPost("/", Create).WithName("CreatePregnancy")
            .WithSummary("Record a new pregnancy")
            .WithDescription("Creates a pregnancy record for a patient with mating date, method, and optional father reference.");
        group.MapGet("/{id:guid}", GetById).WithName("GetPregnancyById")
            .WithSummary("Get pregnancy by ID")
            .WithDescription("Returns the full details of a pregnancy including scheduled checks and current status.");
        group.MapGet("/by-patient/{patientId:guid}", GetByPatient).WithName("GetPregnanciesByPatient")
            .WithSummary("Get pregnancies by patient")
            .WithDescription("Returns all pregnancy records for a specific patient.");
        group.MapGet("/active", GetActive).WithName("GetActivePregnancies")
            .WithSummary("List active pregnancies")
            .WithDescription("Returns all currently active (ongoing) pregnancies across the clinic.");
        group.MapPut("/{id:guid}/delivery", RecordDelivery).WithName("RecordDelivery")
            .WithSummary("Record a delivery")
            .WithDescription("Records the delivery outcome for a pregnancy including date, offspring count, and notes.");
        group.MapPut("/{id:guid}/loss", RecordLoss).WithName("RecordLoss")
            .WithSummary("Record a pregnancy loss")
            .WithDescription("Records a pregnancy loss event with date, outcome classification, and notes.");
        group.MapPost("/{id:guid}/checks", ScheduleCheck).WithName("SchedulePregnancyCheck")
            .WithSummary("Schedule a pregnancy check")
            .WithDescription("Schedules a veterinary check (e.g., ultrasound, blood test) for a pregnancy at a specified date.");
        group.MapPut("/checks/{checkId:guid}/complete", CompleteCheck).WithName("CompletePregnancyCheck")
            .WithSummary("Complete a pregnancy check")
            .WithDescription("Records the result of a scheduled pregnancy check.");

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
