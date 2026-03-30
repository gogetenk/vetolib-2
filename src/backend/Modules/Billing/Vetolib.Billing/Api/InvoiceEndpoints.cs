using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Billing.Application.Commands.AddInvoiceItem;
using Vetolib.Billing.Application.Commands.CreateInvoice;
using Vetolib.Billing.Application.Commands.SubmitToEInvoicing;
using Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Application.Queries.GetEInvoicingStatus;
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
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet", "Receptionist"))
            .WithName("CreateInvoice")
            .WithSummary("Create a new invoice")
            .WithDescription("Creates an invoice for a patient visit with an initial line item. Supports e-invoicing fields for UAE/FR compliance.");

        group.MapGet("/", ListInvoices)
            .WithName("ListInvoices")
            .WithSummary("List invoices")
            .WithDescription("Returns a paginated list of invoices for the current clinic, ordered by most recent first.");

        group.MapGet("/{id:guid}", GetInvoiceById)
            .WithName("GetInvoiceById")
            .WithSummary("Get invoice by ID")
            .WithDescription("Returns the full details of a single invoice including all line items and totals.");

        group.MapPatch("/{id:guid}/status", UpdateInvoiceStatus)
            .WithName("UpdateInvoiceStatus")
            .WithSummary("Update invoice status")
            .WithDescription("Changes the status of an invoice (e.g., Draft, Sent, Paid, Cancelled).");

        group.MapPost("/{id:guid}/items", AddInvoiceItem)
            .WithName("AddInvoiceItem")
            .WithSummary("Add a line item to an invoice")
            .WithDescription("Adds a new line item with description, unit price, and tax category to an existing invoice.");

        group.MapGet("/{id:guid}/pdf", DownloadInvoicePdf)
            .WithName("DownloadInvoicePdf")
            .WithSummary("Download invoice as PDF")
            .WithDescription("Generates and returns the invoice as a PDF file for printing or sharing with the client.");

        group.MapPost("/{id:guid}/submit-einvoicing", SubmitToEInvoicing)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Vet"))
            .WithName("SubmitToEInvoicing")
            .WithSummary("Submit invoice for e-invoicing")
            .WithDescription("Submits the invoice to the e-invoicing platform (Chorus Pro / UAE FTA). Requires Admin or Vet role.");

        group.MapGet("/{id:guid}/einvoicing-status", GetEInvoicingStatus)
            .WithName("GetEInvoicingStatus")
            .WithSummary("Get e-invoicing submission status")
            .WithDescription("Returns the current e-invoicing submission status and any platform response details.");

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
            request.ItemUnitPrice,
            ItemTaxCategory: request.ItemTaxCategory,
            BuyerName: request.BuyerName,
            CountryCode: request.CountryCode,
            InvoiceTypeCode: request.InvoiceTypeCode,
            SellerSiren: request.SellerSiren,
            SellerVatNumber: request.SellerVatNumber,
            BuyerSiren: request.BuyerSiren,
            BuyerVatNumber: request.BuyerVatNumber,
            BuyerAddress: request.BuyerAddress,
            OperationType: request.OperationType,
            PaymentTerms: request.PaymentTerms,
            PurchaseOrderReference: request.PurchaseOrderReference)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListInvoices(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 20)
        => (await sender.Send(new ListInvoicesQuery(pageNumber, pageSize)))
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
        => (await sender.Send(new AddInvoiceItemCommand(id, request.Description, request.UnitPrice, request.TaxCategory)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> DownloadInvoicePdf(
        Guid id,
        ISender sender)
    {
        var result = await sender.Send(new GenerateInvoicePdfQuery(id));
        if (!result.IsSuccess)
            return result.ToMinimalApiResult();

        return Results.File(
            result.Value.PdfBytes,
            contentType: "application/pdf",
            fileDownloadName: $"invoice-{result.Value.InvoiceNumber}.pdf");
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SubmitToEInvoicing(
        Guid id,
        ISender sender)
        => (await sender.Send(new SubmitToEInvoicingCommand(id)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetEInvoicingStatus(
        Guid id,
        ISender sender)
        => (await sender.Send(new GetEInvoicingStatusQuery(id)))
            .ToMinimalApiResult();
}
