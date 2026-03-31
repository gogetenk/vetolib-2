using System.Security.Claims;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.GetOrCreateReferralCode;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class ReferralEndpoints
{
    internal static IEndpointRouteBuilder MapReferralApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/portal/referral-code")
            .WithTags("Referral")
            .RequireAuthorization();

        group.MapGet("/", GetOrCreateReferralCode)
            .WithName("GetOrCreateReferralCode")
            .WithSummary("Get or create the current user's referral code")
            .WithDescription("Returns the authenticated user's referral code. If no code exists yet, one is created automatically.");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetOrCreateReferralCode(
        ClaimsPrincipal user,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Result<ReferralCodeDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new GetOrCreateReferralCodeCommand(userId)))
            .ToMinimalApiResult();
    }
}
