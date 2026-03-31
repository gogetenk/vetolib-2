using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.CreateFollowUpRule;
using Vetolib.Agenda.Application.Commands.DeactivateFollowUpRule;
using Vetolib.Agenda.Application.Commands.UpdateFollowUpRule;
using Vetolib.Agenda.Application.Queries.ListFollowUpRules;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Api;

internal static class FollowUpRuleEndpoints
{
    internal static IEndpointRouteBuilder MapFollowUpRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/follow-up-rules")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("FollowUpRules");

        group.MapGet("/", List)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("ListFollowUpRules")
            .WithSummary("List follow-up rules")
            .WithDescription("Returns all active follow-up rules for the current clinic, ordered by consultation type.");

        group.MapPost("/", Create)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("CreateFollowUpRule")
            .WithSummary("Create a follow-up rule")
            .WithDescription("Defines a new automatic follow-up rule for a consultation type. Requires Admin role.");

        group.MapPut("/{id:guid}", Update)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("UpdateFollowUpRule")
            .WithSummary("Update a follow-up rule")
            .WithDescription("Modifies an existing follow-up rule's consultation type, days, or reason. Requires Admin role.");

        group.MapDelete("/{id:guid}", Deactivate)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("DeactivateFollowUpRule")
            .WithSummary("Deactivate a follow-up rule")
            .WithDescription("Soft-deletes a follow-up rule so it no longer triggers automatic follow-up scheduling. Requires Admin role.");

        return app;
    }

    private static async Task<IResult> List(ISender sender)
        => (await sender.Send(new ListFollowUpRulesQuery())).ToMinimalApiResult();

    private static async Task<IResult> Create(
        CreateFollowUpRuleRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateFollowUpRuleCommand(
            clinicContext.ClinicId,
            request.ConsultationType,
            request.FollowUpDays,
            request.FollowUpReason);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateFollowUpRuleRequest request,
        ISender sender)
    {
        var cmd = new UpdateFollowUpRuleCommand(
            id,
            request.ConsultationType,
            request.FollowUpDays,
            request.FollowUpReason);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> Deactivate(Guid id, ISender sender)
        => (await sender.Send(new DeactivateFollowUpRuleCommand(id))).ToMinimalApiResult();
}
