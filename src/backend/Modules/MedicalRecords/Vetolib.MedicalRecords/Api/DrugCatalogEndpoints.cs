using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Api;

internal static class DrugCatalogEndpoints
{
    internal static IEndpointRouteBuilder MapDrugCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/medical-records/drugs")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("DrugCatalog");

        group.MapGet("/", SearchDrugs)
            .WithName("SearchDrugCatalog");

        group.MapGet("/{id:guid}", GetDrugById)
            .WithName("GetDrugCatalogEntryById");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SearchDrugs(
        ISender sender,
        string? search = null,
        int limit = 20)
    {
        return (await sender.Send(new SearchDrugCatalogQuery(search, limit))).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetDrugById(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetDrugCatalogEntryByIdQuery(id))).ToMinimalApiResult();
    }
}
