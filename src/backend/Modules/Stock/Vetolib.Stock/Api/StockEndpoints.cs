using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Stock.Application.Commands.CreateStockItem;
using Vetolib.Stock.Application.Commands.RecordStockMovement;
using Vetolib.Stock.Application.Commands.UpdateStockItem;
using Vetolib.Stock.Application.Queries.GetStockAlerts;
using Vetolib.Stock.Application.Queries.ListStockItems;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Api;

internal static class StockEndpoints
{
    internal static IEndpointRouteBuilder MapStockApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/stock")
            .RequireAuthorization()
            .WithTags("Stock");

        group.MapGet("/", List).WithName("ListStockItems");
        group.MapPost("/", Create).RequireAuthorization("VetOrAdmin").WithName("CreateStockItem");
        group.MapPatch("/{id:guid}", Update).RequireAuthorization("VetOrAdmin").WithName("UpdateStockItem");
        group.MapPost("/{id:guid}/movements", RecordMovement).RequireAuthorization("VetOrAdmin").WithName("RecordStockMovement");
        group.MapGet("/alerts", GetAlerts).WithName("GetStockAlerts");

        return app;
    }

    private static async Task<IResult> List(
        string? category,
        bool? lowStock,
        bool? expiringSoon,
        ISender sender)
        => (await sender.Send(new ListStockItemsQuery(category, lowStock ?? false, expiringSoon ?? false)))
            .ToMinimalApiResult();

    private static async Task<IResult> Create(
        CreateStockItemRequest req,
        ISender sender)
        => (await sender.Send(new CreateStockItemCommand(
            req.Name, req.Category, req.Quantity, req.Unit, req.MinThreshold, req.ExpiryDate, req.DrugCatalogEntryId)))
            .ToMinimalApiResult();

    private static async Task<IResult> Update(
        Guid id,
        UpdateStockItemRequest req,
        ISender sender)
        => (await sender.Send(new UpdateStockItemCommand(id, req.Name, req.MinThreshold)))
            .ToMinimalApiResult();

    private static async Task<IResult> RecordMovement(
        Guid id,
        CreateStockMovementRequest req,
        ISender sender)
        => (await sender.Send(new RecordStockMovementCommand(id, req.MovementType, req.Quantity, req.Reason)))
            .ToMinimalApiResult();

    private static async Task<IResult> GetAlerts(ISender sender)
        => (await sender.Send(new GetStockAlertsQuery())).ToMinimalApiResult();
}
