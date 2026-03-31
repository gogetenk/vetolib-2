using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateConsultationType;
using Vetolib.Agenda.Application.Commands.DeactivateConsultationType;
using Vetolib.Agenda.Application.Commands.UpdateConsultationType;
using Vetolib.Agenda.Application.Queries.ListConsultationTypes;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Api;

internal static class ConsultationTypeEndpoints
{
    internal static IEndpointRouteBuilder MapConsultationTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/consultation-types")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("ConsultationTypes");

        group.MapPost("/", Create)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("CreateConsultationType")
            .WithSummary("Create a consultation type")
            .WithDescription("Defines a new consultation type with name, duration, and sort order. Requires Admin role.");

        group.MapPut("/{id:guid}", Update)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("UpdateConsultationType")
            .WithSummary("Update a consultation type")
            .WithDescription("Modifies an existing consultation type's name, duration, sort order, or vet selection requirement. Requires Admin role.");

        group.MapDelete("/{id:guid}", Deactivate)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("DeactivateConsultationType")
            .WithSummary("Deactivate a consultation type")
            .WithDescription("Soft-deletes a consultation type so it no longer appears in booking options. Requires Admin role.");

        group.MapGet("/", List)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("ListConsultationTypes")
            .WithSummary("List consultation types")
            .WithDescription("Returns all active consultation types for the current clinic, ordered by sort order.");

        return app;
    }

    private static async Task<IResult> Create(
        CreateConsultationTypeRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateConsultationTypeCommand(
            clinicContext.ClinicId,
            request.Name,
            request.DurationMinutes,
            request.SortOrder,
            request.RequiresVetSelection);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateConsultationTypeRequest request,
        ISender sender)
    {
        var cmd = new UpdateConsultationTypeCommand(
            id,
            request.Name,
            request.DurationMinutes,
            request.SortOrder,
            request.RequiresVetSelection);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> Deactivate(Guid id, ISender sender)
        => (await sender.Send(new DeactivateConsultationTypeCommand(id))).ToMinimalApiResult();

    private static async Task<IResult> List(ISender sender)
        => (await sender.Send(new ListConsultationTypesQuery())).ToMinimalApiResult();
}
