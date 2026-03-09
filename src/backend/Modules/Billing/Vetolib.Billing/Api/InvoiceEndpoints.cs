using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Billing.Application.Commands.AddInvoiceItem;
using Vetolib.Billing.Application.Commands.CreateInvoice;
using Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;
using Vetolib.Billing.Application.Queries.GetInvoiceById;
using Vetolib.Billing.Application.Queries.ListInvoices;
using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Api;

internal static class InvoiceEndpoints
{
    internal static IEndpointRouteBuilder MapInvoiceApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/invoices")
            .RequireAuthorization()
            .WithTags("Invoices");

        group.MapPost("/", CreateInvoice)
            .WithName("CreateInvoice");

        group.MapGet("/", ListInvoices)
            .WithName("ListInvoices");

        group.MapGet("/{id:guid}", GetInvoiceById)
            .WithName("GetInvoiceById");

        group.MapPatch("/{id:guid}/status", UpdateInvoiceStatus)
            .WithName("UpdateInvoiceStatus");

        group.MapPost("/{id:guid}/items", AddInvoiceItem)
            .WithName("AddInvoiceItem");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateInvoice(
        CreateInvoiceRequest request,
        IClinicContext clinicContext,
        ISender sender)
        => (await sender.Send(new CreateInvoiceCommand(
            clinicContext.ClinicId,
            request.AnimalId,
            request.ItemDescription,
            request.ItemUnitPrice)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListInvoices(
        ISender sender)
        => (await sender.Send(new ListInvoicesQuery()))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetInvoiceById(
        Guid id,
        ISender sender)
        => (await sender.Send(new GetInvoiceByIdQuery(id)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> UpdateInvoiceStatus(
        Guid id,
        UpdateInvoiceStatusRequest request,
        ISender sender)
        => (await sender.Send(new UpdateInvoiceStatusCommand(id, request.Status)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> AddInvoiceItem(
        Guid id,
        AddInvoiceItemRequest request,
        ISender sender)
        => (await sender.Send(new AddInvoiceItemCommand(id, request.Description, request.UnitPrice)))
            .ToMinimalApiResult();
}
