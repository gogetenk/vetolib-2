using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Agenda.Application.Commands.SubmitVisitFeedback;
using Vetolib.Agenda.Application.Queries.GetVisitFeedbackStats;
using Vetolib.Agenda.Application.Queries.ListVisitFeedback;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Api;

internal static class FeedbackEndpoints
{
    internal static IEndpointRouteBuilder MapFeedbackApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Submit feedback on a specific appointment
        var appointmentsGroup = app.MapGroup("/api/v1/appointments")
            .RequireAuthorization()
            .WithTags("Feedback");

        appointmentsGroup.MapPost("/{id:guid}/feedback", SubmitFeedback)
            .WithName("SubmitVisitFeedback")
            .WithSummary("Submit feedback for a completed appointment")
            .WithDescription("Allows a pet owner to rate and comment on a completed visit. Only one feedback per appointment is allowed.");

        // Admin feedback endpoints
        var feedbackGroup = app.MapGroup("/api/v1/feedback")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithTags("Feedback");

        feedbackGroup.MapGet("/", ListFeedback)
            .WithName("ListVisitFeedback")
            .WithSummary("List all feedback for the clinic")
            .WithDescription("Returns all visit feedback for the current clinic, ordered by most recent first. Requires Admin role.");

        feedbackGroup.MapGet("/stats", GetFeedbackStats)
            .WithName("GetVisitFeedbackStats")
            .WithSummary("Get feedback statistics")
            .WithDescription("Returns average rating, count by star, and NPS score for the current clinic. Requires Admin role.");

        return app;
    }

    private static async Task<IResult> SubmitFeedback(
        Guid id,
        SubmitVisitFeedbackRequest request,
        ISender sender)
    {
        var cmd = new SubmitVisitFeedbackCommand(
            id,
            request.Rating,
            request.Comment,
            request.IsPublic);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListFeedback(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 50)
    {
        return (await sender.Send(new ListVisitFeedbackQuery(pageNumber, pageSize))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetFeedbackStats(ISender sender)
    {
        return (await sender.Send(new GetVisitFeedbackStatsQuery())).ToMinimalApiResult();
    }
}
