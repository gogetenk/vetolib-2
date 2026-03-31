using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalPrescriptions;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalRecords;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalVaccinations;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalWeightHistory;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetMyAnimals;

namespace Vetolib.MedicalRecords.Api;

internal static class OwnerPortalEndpoints
{
    internal static IEndpointRouteBuilder MapOwnerPortalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/portal")
            .RequireAuthorization()
            .WithTags("OwnerPortal");

        group.MapGet("/my-animals", GetMyAnimals)
            .WithName("GetMyAnimals")
            .WithSummary("List owner's animals across all clinics")
            .WithDescription("Returns all animals linked to the authenticated owner account, across all clinics. Requires owner JWT with owner_account_id claim.");

        group.MapGet("/animals/{id:guid}/records", GetAnimalRecords)
            .WithName("GetAnimalRecords")
            .WithSummary("Get medical records for an animal")
            .WithDescription("Returns medical records visible to the owner for a specific animal. Only records with IsVisibleToOwner=true are returned.");

        group.MapGet("/animals/{id:guid}/vaccinations", GetAnimalVaccinations)
            .WithName("GetAnimalVaccinations")
            .WithSummary("Get vaccination history for an animal")
            .WithDescription("Returns the vaccination history (prescriptions from visible records) for a specific animal.");

        group.MapGet("/animals/{id:guid}/prescriptions", GetAnimalPrescriptions)
            .WithName("GetAnimalPrescriptions")
            .WithSummary("Get active prescriptions for an animal")
            .WithDescription("Returns all prescriptions from visible medical records for a specific animal.");

        group.MapGet("/animals/{id:guid}/weight", GetAnimalWeightHistory)
            .WithName("GetAnimalWeightHistory")
            .WithSummary("Get weight history for an animal")
            .WithDescription("Returns the weight measurement history for a specific animal.");

        return app;
    }

    private static Guid? GetOwnerAccountId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("owner_account_id")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static async Task<IResult> GetMyAnimals(
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = GetOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new GetMyAnimalsQuery(ownerAccountId.Value))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAnimalRecords(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = GetOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new GetAnimalRecordsQuery(ownerAccountId.Value, id))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAnimalVaccinations(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = GetOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new GetAnimalVaccinationsQuery(ownerAccountId.Value, id))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAnimalPrescriptions(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = GetOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new GetAnimalPrescriptionsQuery(ownerAccountId.Value, id))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetAnimalWeightHistory(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = GetOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new GetAnimalWeightHistoryQuery(ownerAccountId.Value, id))).ToMinimalApiResult();
    }
}
