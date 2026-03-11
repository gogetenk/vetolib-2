using System.Security.Claims;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpIResult = Microsoft.AspNetCore.Http.IResult;
using Vetolib.Auth.Application.Commands.CompleteOnboardingStep;
using Vetolib.Auth.Application.Commands.DismissChecklist;
using Vetolib.Auth.Application.Commands.DismissWelcomeBanner;
using Vetolib.Auth.Application.Queries.GetOnboardingState;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class OnboardingEndpoints
{
    internal static IEndpointRouteBuilder MapOnboardingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/onboarding")
            .RequireAuthorization()
            .WithTags("Onboarding");

        group.MapGet("/", GetState).WithName("GetOnboardingState");
        group.MapPost("/steps/{stepId}/complete", CompleteStep).WithName("CompleteOnboardingStep");
        group.MapPost("/banner/dismiss", DismissBanner).WithName("DismissWelcomeBanner");
        group.MapPost("/checklist/dismiss", DismissChecklist).WithName("DismissChecklist");

        return app;
    }

    private static async Task<HttpIResult> GetState(ClaimsPrincipal user, ISender sender)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Result<OnboardingStateDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new GetOnboardingStateQuery(userId.Value))).ToMinimalApiResult();
    }

    private static async Task<HttpIResult> CompleteStep(string stepId, ClaimsPrincipal user, ISender sender)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new CompleteOnboardingStepCommand(userId.Value, stepId))).ToMinimalApiResult();
    }

    private static async Task<HttpIResult> DismissBanner(ClaimsPrincipal user, ISender sender)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new DismissWelcomeBannerCommand(userId.Value))).ToMinimalApiResult();
    }

    private static async Task<HttpIResult> DismissChecklist(ClaimsPrincipal user, ISender sender)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new DismissChecklistCommand(userId.Value))).ToMinimalApiResult();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (claim is null || !Guid.TryParse(claim, out var id))
            return null;

        return id;
    }
}
