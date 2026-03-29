using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Vetolib.Breeding.Api;

internal static class BreedingEndpoints
{
    internal static IEndpointRouteBuilder MapBreedingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/breeding")
            .RequireAuthorization()
            .WithTags("Breeding");

        // Endpoints will be added by subsequent tasks

        return app;
    }
}
