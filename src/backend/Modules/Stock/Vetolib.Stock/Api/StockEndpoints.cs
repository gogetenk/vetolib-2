using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
            .RequireRateLimiting("api")
            .WithTags("Stock");

        group.MapGet("/", List).WithName("ListStockItems")
            .WithSummary("List stock items")
            .WithDescription("Returns a paginated list of stock items, filterable by category, low stock, and expiring soon.");
        group.MapPost("/", Create).RequireAuthorization("VetOrAdmin").WithName("CreateStockItem")
            .WithSummary("Create a stock item")
            .WithDescription("Adds a new item to the stock inventory with quantity, unit, threshold, and optional expiry date.");
        group.MapPatch("/{id:guid}", Update).RequireAuthorization("VetOrAdmin").WithName("UpdateStockItem")
            .WithSummary("Update a stock item")
            .WithDescription("Updates the name or minimum threshold of an existing stock item.");
        group.MapPost("/{id:guid}/movements", RecordMovement).RequireAuthorization("VetOrAdmin").WithName("RecordStockMovement")
            .WithSummary("Record a stock movement")
            .WithDescription("Records a stock movement (intake, consumption, adjustment, or disposal) with quantity and reason.");
        group.MapGet("/alerts", GetAlerts).CacheOutput("Moderate2min").WithName("GetStockAlerts")
            .WithSummary("Get stock alerts")
            .WithDescription("Returns items that are below minimum threshold or expiring soon.");

        return app;
    }

    private static async Task<IResult> List(
        string? category,
        bool? lowStock,
        bool? expiringSoon,
        int? pageNumber,
        int? pageSize,
        ISender sender)
    {
        var size = pageSize ?? 50;
        if (size is < 1 or > 200) size = 50;
        return (await sender.Send(new ListStockItemsQuery(category, lowStock ?? false, expiringSoon ?? false, pageNumber ?? 1, size)))
            .ToMinimalApiResult();
    }

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
